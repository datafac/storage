using System;
using System.CommandLine;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DTOMaker.CLI
{
    internal interface ITargetLanguage
    {
        string Name { get; }
        string PrefixComment { get; }
        string PrefixMetaCode { get; }
        string EmitCodePrefix { get; }
        string EmitCodeSuffix { get; }
        string EmitFileHeader { get; }
        string EmitFileFooter { get; }
    }

    internal sealed class TargetLanguage_CSharp : ITargetLanguage
    {
        private static readonly TargetLanguage_CSharp _instance = new TargetLanguage_CSharp();
        public static ITargetLanguage Instance => _instance;

        private static readonly string _header =
            """
            using System;
            using System.Linq;
            using DTOMaker.Gentime;
            namespace _targetNamespace_;
            #pragma warning disable CS0162 // Unreachable code detected
            public sealed class EntityGenerator : EntityGeneratorBase
            {
                public EntityGenerator(ILanguage language) : base(language) { }
                protected override void OnGenerate(ModelScopeEntity entity)
                {
            """;

        private static readonly string _footer =
            """
                }
            }
            """;

        public string Name => "CSharp";
        public string PrefixComment => "//";
        public string PrefixMetaCode => "##";
        public string EmitCodePrefix => "Emit(\"";
        public string EmitCodeSuffix => "\");";

        public string EmitFileHeader => _header;
        public string EmitFileFooter => _footer;

        private TargetLanguage_CSharp() { }
    }

    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var t2gCommand = new Command("t2g", "Builds generators from templates");
            var template = new Option<FileInfo>(["--source", "-s"])
            {
                IsRequired = true,
                Description = "The source template file to consume",
            };
            t2gCommand.AddOption(template);

            var generator = new Option<FileInfo>(["--output", "-o"])
            {
                IsRequired = true,
                Description = "The output generator file to produce",
            };
            t2gCommand.AddOption(generator);

            var targetNamespace = new Option<string>(["--namespace", "-n"])
            {
                IsRequired = true,
                Description = "The output namespace of the generator",
            };
            t2gCommand.AddOption(targetNamespace);

            var language = new Option<string>(["--language", "-l"], () => "cs")
            {
                IsRequired = true,
                Description = "The target language file extension (default: cs)",
            };
            t2gCommand.AddOption(language);

            t2gCommand.SetHandler<FileInfo, FileInfo, string, string>(T2GHandler, template, generator, targetNamespace, language);

            var rootCommand = new RootCommand("dtomaker")
            {
                t2gCommand
            };

            rootCommand.TreatUnmatchedTokensAsErrors = true;
            return await rootCommand.InvokeAsync(args);
        }

        private static async Task<int> T2GHandler(FileInfo source, FileInfo output, string targetNamespace, string languageExtn)
        {
            Console.WriteLine($"{ThisAssembly.AssemblyTitle} ({ThisAssembly.AssemblyInformationalVersion})");
            Console.WriteLine("T2G: Creating generator...");
            Console.WriteLine($"T2G:   Source   : {source.FullName}");
            Console.WriteLine($"T2G:   Output   : {output.FullName}");
            Console.WriteLine($"T2G:   Namespace: {targetNamespace}");
            try
            {
                ITargetLanguage language = languageExtn.ToLower() switch
                {
                    "cs" => TargetLanguage_CSharp.Instance,
                    _ => throw new ArgumentOutOfRangeException("language", languageExtn, null),
                };
                Console.WriteLine($"T2G:   Language : {language.Name}");

                using var fs = output.Create();
                using var sw = new StreamWriter(fs);
                await sw.WriteLineAsync(language.EmitFileHeader.Replace("_targetNamespace_", targetNamespace));
                int lineNumber = 0;
                await foreach (var inputLine in File.ReadLinesAsync(source.FullName, CancellationToken.None))
                {
                    lineNumber++;
                    string outputLine = T2GConvertLine(inputLine, language);
                    await sw.WriteLineAsync(outputLine);
                }
                await sw.WriteLineAsync(language.EmitFileFooter);
                Console.WriteLine($"T2G: Generator created ({lineNumber} lines)");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"T2G: Error: {e.GetType().Name}: {e.Message}");
                return -1;
            }
        }

        private static string T2GConvertLine(ReadOnlySpan<char> input, ITargetLanguage language)
        {
            int outerIndentPos = input.SizeOfLeadingWhitespace();
            var outerIndent = input.Slice(0, outerIndentPos);
            var sourceCode = input.Slice(outerIndentPos);

            StringBuilder result = new StringBuilder();
            if (sourceCode.StartsWith(language.PrefixComment))
            {
                // comment found
                var comment = sourceCode.Slice(language.PrefixComment.Length);
                int innerIndentPos = comment.SizeOfLeadingWhitespace();
                var innerIndent = comment.Slice(0, innerIndentPos);
                var candidate = comment.Slice(innerIndentPos);
                if (candidate.StartsWith(language.PrefixMetaCode))
                {
                    // metacode found - emit
                    var metacode = candidate.Slice(language.PrefixMetaCode.Length);
                    result.Append(outerIndent);
                    result.Append(innerIndent);
                    result.Append(metacode);
                    return result.ToString();
                }
            }

            result.Append(language.EmitCodePrefix);
            result.Append(outerIndent);
            result.AppendEscaped(sourceCode);
            result.Append(language.EmitCodeSuffix);
            return result.ToString();
        }
    }
}
