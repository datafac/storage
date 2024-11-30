using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DTOMaker.CSPoco
{
    [Generator(LanguageNames.CSharp)]
    public class CSPocoSourceGenerator : ISourceGenerator
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
                            DiagnosticId.DMMP0001, 
                            "Missing assembly reference",
                            $"The generated code requires a reference to {packageName} (v{packageVersion} or later).",
                            DiagnosticCategory.Other,
                            DiagnosticSeverity.Warning,
                            true),
                            Location.None));
            }
        }

        private string GenerateSourceText(ILanguage language, IModelScope outerScope, string templateName)
        {
            var template = Assembly.GetExecutingAssembly().GetTemplate(templateName);
            var processor = new TemplateProcessor();
            var builder = new StringBuilder();
            foreach (string line in processor.ProcessTemplate(template, language, outerScope))
            {
                builder.AppendLine(line);
            }
            return builder.ToString();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver syntaxReceiver) return;

            //// check that the users compilation references the expected libraries
            //CheckReferencedAssemblyNamesInclude(context, typeof(DTOMaker.Models.DomainAttribute).Assembly);

            Version fv = new Version(ThisAssembly.AssemblyFileVersion);
            string shortVersion = $"{fv.Major}.{fv.Minor}";

            foreach (var domain in syntaxReceiver.Domains.Values)
            {
                EmitDiagnostics(context, domain);

                var language = Language_CSharp.Instance;
                var domainScope = new ModelScope_Domain(language, domain, Array.Empty<KeyValuePair<string, object?>>());
                //var domainTokens = ImmutableDictionary<string, object?>.Empty
                //    .Add("DomainName", domain.Name);

                // common/domain code
                {
                    string sourceText = GenerateSourceText(language, domainScope, "DTOMaker.CSPoco.DomainTemplate.cs");
                    context.AddSource(
                        $"{domain.Name}.EntityBase.CSPoco.g.cs",
                        sourceText);
                }

                foreach (var entity in domain.Entities.Values.OrderBy(e => e.Name))
                {
                    // run checks
                    EmitDiagnostics(context, entity);
                    foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
                    {
                        EmitDiagnostics(context, member);
                    }

                    var entityScope = new ModelScope_Entity(language, entity, domainScope.Variables);
                    string sourceText = GenerateSourceText(language, entityScope, "DTOMaker.CSPoco.EntityTemplate.cs");
                    context.AddSource(
                        $"{domain.Name}.{entity.Name}.CSPoco.g.cs",
                        sourceText);
                }
            }
        }
    }
}
