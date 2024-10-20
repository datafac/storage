using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DTOMaker.MemBlocks
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
        private void CheckReferencedAssemblyNamesInclude(GeneratorExecutionContext context, Assembly assembly)
        {
            string packageName = assembly.GetName().Name;
            Version packageVersion = assembly.GetName().Version;
            if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase)))
            {
                // todo major version error/minor version warning
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

        private static int GetFieldLength(TargetMember member)
        {
            string typeName = member.MemberWireTypeName;
            switch (typeName)
            {
                case "Boolean":
                case "Byte":
                case "SByte":
                    return 1;
                case "Int16":
                case "UInt16":
                case "Char":
                case "Half":
                    return 2;
                case "Int32":
                case "UInt32":
                case "Single":
                    return 4;
                case "Int64":
                case "UInt64":
                case "Double":
                    return 8;
                case "Int128":
                case "UInt128":
                case "Guid":
                case "Decimal":
                    return 16;
                default:
                    member.SyntaxErrors.Add(
                        new SyntaxDiagnostic(
                            DiagnosticId.DMMB0007, "Unsupported member type", DiagnosticCategory.Design, member.Location, DiagnosticSeverity.Error,
                            $"Unsupported member type: '{typeName}'"));
                    return 0;
            }
        }

        private static void AutoLayoutMembers(TargetEntity entity)
        {
            switch (entity.LayoutMethod)
            {
                case Models.LayoutMethod.Explicit:
                    ExplicitLayoutMembers(entity);
                    break;
                case Models.LayoutMethod.SequentialV1:
                    SequentialLayoutMembers(entity);
                    break;
            }
        }

        /// <summary>
        /// Calculates length for explicitly positioned members
        /// </summary>
        /// <param name="entity"></param>
        private static void ExplicitLayoutMembers(TargetEntity entity)
        {
            foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
            {
                member.FieldLength = GetFieldLength(member);
                // todo allocate Flags byte
            }
        }

        /// <summary>
        /// Calculates offset and length for all members in sequential order
        /// </summary>
        /// <param name="entity"></param>
        private static void SequentialLayoutMembers(TargetEntity entity)
        {
            int minBlockLength = 0;
            int fieldOffset = 0;

            int Allocate(int fieldLength)
            {
                // calculate this offset
                while (fieldLength > 0 && fieldOffset % fieldLength != 0)
                {
                    fieldOffset++;
                }
                int result = fieldOffset;

                // calc next offset
                fieldOffset = fieldOffset + fieldLength;
                while (fieldOffset > minBlockLength)
                {
                    minBlockLength = minBlockLength == 0 ? 1 : minBlockLength * 2;
                }

                return result;
            }

            foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
            {
                // allocate value bytes
                int fieldLength = GetFieldLength(member);
                member.FieldLength = fieldLength;
                member.FieldOffset = Allocate(fieldLength);

                // allocate flags byte
                member.FlagsOffset = Allocate(1);

                // todo allocate count bytes
                // member.CountOffset = Allocate(2); // ushort

            }
            entity.BlockLength = minBlockLength;
        }

        private static string[] GetTemplate(string templateName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(templateName);
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
            CheckReferencedAssemblyNamesInclude(context, typeof(DTOMaker.Runtime.IFieldCodec).Assembly);

            Version fv = new Version(ThisAssembly.AssemblyFileVersion);
            string shortVersion = $"{fv.Major}.{fv.Minor}";

            foreach (var domain in syntaxReceiver.Domains.Values)
            {
                EmitDiagnostics(context, domain);
                var domainTokens = ImmutableDictionary<string, object?>.Empty
                    .Add("DomainName", domain.Name);
                foreach (var entity in domain.Entities.Values.OrderBy(e => e.Name))
                {
                    // do any auto-layout if required
                    AutoLayoutMembers(entity);

                    // run checks
                    EmitDiagnostics(context, entity);
                    foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
                    {
                        EmitDiagnostics(context, member);
                    }

                    string hintName = $"{domain.Name}.{entity.Name}.MemBlocks.g.cs";
                    var builder = new StringBuilder();
                    var template = GetTemplate("DTOMaker.MemBlocks.EntityTemplate.cs");
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
