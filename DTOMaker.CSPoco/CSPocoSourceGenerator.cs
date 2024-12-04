using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;

namespace DTOMaker.CSPoco
{
    public class CSPocoSourceGenerator : SourceGeneratorBase
    {
        protected override void OnInitialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new CSPocoSyntaxReceiver());
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

        protected override void OnExecute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not CSPocoSyntaxReceiver syntaxReceiver) return;

            //// check that the users compilation references the expected libraries
            //CheckReferencedAssemblyNamesInclude(context, typeof(DTOMaker.Models.DomainAttribute).Assembly);

            var assembly = Assembly.GetExecutingAssembly();
            var language = Language_CSharp.Instance;
            //Version fv = new Version(ThisAssembly.AssemblyFileVersion);
            //string shortVersion = $"{fv.Major}.{fv.Minor}";

            foreach (var domain in syntaxReceiver.Domains.Values)
            {
                EmitDiagnostics(context, domain);

                var domainScope = new ModelScope_Domain(language, domain);

                // emit base entity
                {
                    string sourceText = GenerateSourceText(language, domainScope, assembly, "DTOMaker.CSPoco.DomainTemplate.cs");
                    context.AddSource(
                        $"{domain.Name}.EntityBase.CSPoco.g.cs",
                        sourceText);
                }

                // emit each entity
                foreach (var entity in domain.Entities.Values.OrderBy(e => e.Name))
                {
                    // run checks
                    EmitDiagnostics(context, entity);
                    foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
                    {
                        EmitDiagnostics(context, member);
                    }

                    var entityScope = new ModelScopeEntity(domainScope, language, entity);
                    string sourceText = GenerateSourceText(language, entityScope, assembly, "DTOMaker.CSPoco.EntityTemplate.cs");
                    context.AddSource(
                        $"{domain.Name}.{entity.Name}.CSPoco.g.cs",
                        sourceText);
                }
            }
        }
    }
}
