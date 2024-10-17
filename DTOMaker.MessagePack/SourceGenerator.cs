using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DTOMaker.MessagePack
{
    [Generator(LanguageNames.CSharp)]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private void EmitDiagnostics(GeneratorExecutionContext context, TargetBase target)
        {
            // todo fix msg ids
            foreach (var diagnostic in target.SyntaxErrors)
            {
                // report diagnostic
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.Message,
                            diagnostic.Category, diagnostic.Severity, true), diagnostic.Location));
            }
            foreach (var diagnostic in target.ValidationErrors())
            {
                // report diagnostic
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.Message,
                            diagnostic.Category, diagnostic.Severity, true), diagnostic.Location));
            }
        }
        private void CheckReferencedAssemblyNamesInclude(GeneratorExecutionContext context, Assembly assembly)
        {
            string packageName = assembly.GetName().Name;
            Version packageVersion = assembly.GetName().Version;
            if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase)))
            {
                // todo major version error/minor version warning
                // todo fix diag id, title and categ
                context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "MFNSSG001", "DiagnosticTitle",
                            $"The generated code requires a reference to {packageName} (v{packageVersion} or later).",
                            "DiagnosticCategory",
                            DiagnosticSeverity.Warning,
                            true),
                            Location.None));
            }
        }

        private static string[] GetTemplate(Assembly assembly, string templateName)
        {
            using var stream = assembly.GetManifestResourceStream(templateName);
            if (stream is null) throw new ArgumentException($"Template '{templateName}' not found", nameof(templateName));
            var result = new List<string>();
            using var reader = new StreamReader(stream);
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                result.Add(line);
            }
            return result.ToArray();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver syntaxReceiver) return;

            // check that the users compilation references the expected libraries
            CheckReferencedAssemblyNamesInclude(context, typeof(Models.DomainAttribute).Assembly);

            Version fv = new Version(ThisAssembly.AssemblyFileVersion);
            string shortVersion = $"{fv.Major}.{fv.Minor}";

            foreach (var domain in syntaxReceiver.Domains.Values)
            {
                EmitDiagnostics(context, domain);
                var domainTokens = ImmutableDictionary<string, object?>.Empty
                    .Add("DomainName", domain.Name);
                foreach (var entity in domain.Entities.Values.OrderBy(e => e.Name))
                {
                    // run checks
                    EmitDiagnostics(context, entity);
                    foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
                    {
                        EmitDiagnostics(context, member);
                    }

                    string hintName = $"{domain.Name}.{entity.Name}.MessagePack.g.cs";
                    var builder = new StringBuilder();
                    var template = GetTemplate(Assembly.GetExecutingAssembly(), "DTOMaker.MessagePack.EntityTemplate.cs");
                    var processor = new TemplateProcessor();
                    var language = Language_CSharp.Instance;
                    var outerScope = new ModelScope_Entity(language, entity, domainTokens);
                    foreach (string line in processor.ProcessTemplate(template, language, outerScope))
                    {
                        builder.AppendLine(line);
                    }
                    context.AddSource(hintName, builder.ToString());
                }
            }
        }
    }
}
