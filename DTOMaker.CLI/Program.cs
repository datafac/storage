using System;
using System.CommandLine;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DTOMaker.CLI
{
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

            t2gCommand.SetHandler<FileInfo, FileInfo, string>(T2GHandler, template, generator, targetNamespace);

            var rootCommand = new RootCommand("dtomaker")
            {
                t2gCommand
            };

            rootCommand.TreatUnmatchedTokensAsErrors = true;
            return await rootCommand.InvokeAsync(args);
        }

        private static readonly string Header =
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

        private static readonly string Footer =
            """
                }
            }
            """;

        private static async Task<int> T2GHandler(FileInfo source, FileInfo output, string targetNamespace)
        {
            Console.WriteLine($"{ThisAssembly.AssemblyTitle} ({ThisAssembly.AssemblyInformationalVersion})");
            Console.WriteLine("T2G: Creating generator...");
            Console.WriteLine($"T2G:   Source   : {source.FullName}");
            Console.WriteLine($"T2G:   Output   : {output.FullName}");
            Console.WriteLine($"T2G:   Namespace: {targetNamespace}");
            try
            {
                using var fs = output.Create();
                using var sw = new StreamWriter(fs);
                await sw.WriteLineAsync(Header.Replace("_targetNamespace_", targetNamespace));
                int lineNumber = 0;
                await foreach (var inputLine in File.ReadLinesAsync(source.FullName, CancellationToken.None))
                {
                    lineNumber++;
                    string outputLine = T2GConvertLine(lineNumber, inputLine);
                    await sw.WriteLineAsync(outputLine);
                }
                await sw.WriteLineAsync(Footer);
                Console.WriteLine($"T2G: Generator created ({lineNumber} lines)");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"T2G: Error: {e.GetType().Name}: {e.Message}");
                return -1;
            }
        }

        private static int SizeOfIndent(ReadOnlySpan<char> input)
        {
            for (int pos = 0; pos < input.Length; pos++)
            {
                if (!char.IsWhiteSpace(input[pos])) return pos;
            }
            return 0;
        }

        private static (string indent, string remainder) StripLeadingWhitespace(string input)
        {
            StringBuilder indent = new StringBuilder();
            for (int pos = 0; pos < input.Length; pos++)
            {
                char ch = input[pos];
                if (char.IsWhiteSpace(ch))
                {
                    // nothing
                    indent.Append(ch);
                }
                else
                {
                    return (indent.ToString(), input.Substring(pos));
                }
            }
            return (input, "");
        }

        // todo make language agnostic
        private const string PrefixCSharpComment = "//";
        private const string PrefixMetaCode = "##";
        private const string EmitCodePrefix = "Emit(\"";
        private const string EmitCodeSuffix = "\");";

        private static string T2GConvertLine(int lineNumber, string input)
        {
            // todo spanify
            ReadOnlySpan<char> inputSpan = input;
            int indentSize = SizeOfIndent(inputSpan);
            ReadOnlySpan<char> outerIndentSpan = inputSpan.Slice(0, indentSize);
            ReadOnlySpan<char> sourceCodeSpan = inputSpan.Slice(indentSize);

            (string outerIndent, string sourceCode) = StripLeadingWhitespace(input);
            if (sourceCode.StartsWith(PrefixCSharpComment))
            {
                // comment found
                string comment = sourceCode.Substring(PrefixCSharpComment.Length);
                (string innerIndent, string candidate) = StripLeadingWhitespace(comment);
                if (candidate.StartsWith(PrefixMetaCode))
                {
                    // metacode found - emit
                    string metacode = candidate.Substring(PrefixMetaCode.Length);
                    return $"{outerIndent}{innerIndent}{metacode}";
                }
            }
            string encodedSource = EncodeSource(outerIndent, sourceCode);
            return $"{EmitCodePrefix}{encodedSource}{EmitCodeSuffix}";
        }

        private static string EncodeSource(string outerIndent, string sourceCode)
        {
            return $"{outerIndent}{Escaped(sourceCode)}";
        }
        private static string? Escaped(string input)
        {
            if (input is null) return null;
            // escape double quotes
            StringBuilder result = new StringBuilder();
            foreach (char ch in input)
            {
                if (ch == '"') // || ch == '\\')
                    result.Append('\\');
                result.Append(ch);
            }
            return result.ToString();
        }
    }
}
