using Microsoft.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DTOMaker.Gentime
{
    public abstract class SourceGeneratorBase : ISourceGenerator
    {
        protected abstract void OnInitialize(GeneratorInitializationContext context);
        public void Initialize(GeneratorInitializationContext context) => OnInitialize(context);

        protected abstract void OnExecute(GeneratorExecutionContext context);
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiverBase syntaxReceiver) return;

            // fix entity hierarchy
            foreach (var domain in syntaxReceiver.Domains.Values)
            {
                foreach (var entity in domain.Entities.Values.ToArray())
                {
                    if (entity.BaseName is not null && entity.BaseName != "EntityBase")
                    {
                        if (domain.Entities.TryGetValue(entity.BaseName, out var baseEntity))
                        {
                            entity.Base = baseEntity;
                        }
                        else
                        {
                            // invalid base name!
                            entity.SyntaxErrors.Add(
                                new SyntaxDiagnostic(
                                    DiagnosticId.DTOM0008, "Invalid base name", DiagnosticCategory.Design, entity.Location, DiagnosticSeverity.Error,
                                    $"Base name '{entity.BaseName}' does not refer to a known entity."));
                        }
                    }
                }
            }

            OnExecute(context);
        }

        protected string GenerateSourceText(ILanguage language, IModelScope outerScope, Assembly assembly, string templateName)
        {
            var template = assembly.GetTemplate(templateName);
            var processor = new TemplateProcessor();
            var builder = new StringBuilder();
            foreach (string line in processor.ProcessTemplate(template, language, outerScope))
            {
                builder.AppendLine(line);
            }
            return builder.ToString();
        }
    }
}
