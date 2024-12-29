using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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

        private static int GetFieldLength(TargetMember member)
        {
            string typeName = member.MemberTypeName;
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
                case "String":
                    // encoded as UTF8
                    return 1;
                default:
                    return 0;
            }
        }

        private static void AutoLayoutMembers(MemBlockEntity entity)
        {
            switch (entity.LayoutMethod)
            {
                case LayoutMethod.Explicit:
                    ExplicitLayoutMembers(entity);
                    break;
                case LayoutMethod.SequentialV1:
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
            }
        }

        /// <summary>
        /// Calculates offset and length for all members in sequential order
        /// </summary>
        /// <param name="entity"></param>
        private static void SequentialLayoutMembers(TargetEntity baseEntity)
        {
            if (baseEntity is not MemBlockEntity entity) return;

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
                // adjust field/array length for String types
                if(member.MemberTypeName == "String")
                {
                    fieldLength = member.ArrayLength;
                    member.ArrayLength = 0;
                }

                member.FieldLength = fieldLength;
                if (member.MemberIsArray)
                {
                    member.FieldOffset = Allocate(fieldLength * member.ArrayLength);
                }
                else
                {
                    member.FieldOffset = Allocate(fieldLength);
                }
            }
            entity.BlockLength = minBlockLength;
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

            // emit each entity
            foreach (var entity in domain.Entities.Values.OrderBy(e => e.EntityName.FullName).OfType<MemBlockEntity>())
            {
                // do any auto-layout if required
                AutoLayoutMembers(entity);

                EmitDiagnostics(context, entity);
                foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
                {
                    EmitDiagnostics(context, member);
                }

                var entityScope = factory.CreateEntity(domainScope, factory, language, entity);
                string sourceText = GenerateSourceText(language, entityScope, assembly, "DTOMaker.MemBlocks.EntityTemplate.cs");
                context.AddSource(
                    $"{entity.EntityName.FullName}.MemBlocks.g.cs",
                    sourceText);
            }
        }
    }
}
