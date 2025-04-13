using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace DTOMaker.MemBlocks
{
    [Generator(LanguageNames.CSharp)]
    public class MemBlocksSourceGenerator : SourceGeneratorBase
    {

        protected override void OnInitialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MemBlocksSyntaxReceiver());
        }

        private void EmitDiagnostics(GeneratorExecutionContext context, TargetBase target)
        {
            foreach (var diagnostic in target.SyntaxErrors)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            diagnostic.Id, diagnostic.Title, diagnostic.Message, diagnostic.Category, diagnostic.Severity, true), diagnostic.Location));
            }
            foreach (var diagnostic in target.ValidationErrors())
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            diagnostic.Id, diagnostic.Title, diagnostic.Message, diagnostic.Category, diagnostic.Severity, true), diagnostic.Location));
            }
        }

        protected override void OnExecute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not MemBlocksSyntaxReceiver syntaxReceiver) return;

            var assembly = Assembly.GetExecutingAssembly();
            var language = Language_CSharp.Instance;
            var factory = new MemBlocksScopeFactory();

            var domain = syntaxReceiver.Domain;
            EmitDiagnostics(context, domain);

            var domainScope = new MemBlocksModelScopeDomain(ModelScopeEmpty.Instance, factory, language, domain);

            // complete intra-entity layout
            foreach (var entity in domain.Entities.Values.OrderBy(e => e.TFN.FullName).OfType<MemBlockEntity>())
            {
                entity.AutoLayoutMembers();
            }

            // complete inter-entity layout
            foreach (var entity in domain.Entities.Values.OrderBy(e => e.TFN.FullName).OfType<MemBlockEntity>())
            {
                entity.BuildStructureCodes();
            }

            // emit each entity
            foreach (var entity in domain.Entities.Values.OrderBy(e => e.TFN.FullName).OfType<MemBlockEntity>())
            {
                EmitDiagnostics(context, entity);
                foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
                {
                    EmitDiagnostics(context, member);
                }

                var entityScope = factory.CreateEntity(domainScope, factory, language, entity);

                var generator = new EntityGenerator(language);
                string sourceText = generator.GenerateSourceText(entityScope);

                context.AddSource($"{entity.TFN.FullName}.MemBlocks.g.cs", sourceText);
            }
        }
    }
}
