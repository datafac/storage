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
            string typeName = member.MemberType.FullName;
            switch (typeName)
            {
                case "System.Boolean":
                case "System.Byte":
                case "System.SByte":
                    return 1;
                case "System.Int16":
                case "System.UInt16":
                case "System.Char":
                case "System.Half":
                    return 2;
                case "System.Int32":
                case "System.UInt32":
                case "System.Single":
                    return 4;
                case "System.Int64":
                case "System.UInt64":
                case "System.Double":
                    return 8;
                case "System.Int128":
                case "System.UInt128":
                case "System.Guid":
                case "System.Decimal":
                    return 16;
                case "System.String":
                    // encoded as UTF8
                    return 1;
                // todo case "DataFac.Octets":
                //    return 1;
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
                case LayoutMethod.Linear:
                    LinearLayoutMembers(entity);
                    break;
            }
        }

        /// <summary>
        /// Calculates length for explicitly positioned members
        /// </summary>
        /// <param name="entity"></param>
        private static void ExplicitLayoutMembers(TargetEntity entity)
        {
            foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence).OfType<MemBlockMember>())
            {
                member.FieldLength = GetFieldLength(member);
            }
        }

        /// <summary>
        /// Calculates offset and length for all members in linear order
        /// </summary>
        /// <param name="entity"></param>
        private static void LinearLayoutMembers(TargetEntity baseEntity)
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

            foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence).OfType<MemBlockMember>())
            {
                // allocate value bytes
                int fieldLength = GetFieldLength(member);
                // adjust field/array length for String types
                if(member.MemberType.FullName == "System.String")
                {
                    fieldLength = member.StringLength;
                }
                else if (member.MemberIsEntity)
                {
                    fieldLength = 64; // todo get sizeof BlobIdV0
                }

                member.FieldLength = fieldLength;
                if (member.MemberIsVector)
                {
                    member.FieldOffset = Allocate(fieldLength * member.ArrayCapacity);
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

                var generator = new EntityGenerator(language);
                string sourceText = generator.GenerateSourceText(entityScope);

                context.AddSource($"{entity.EntityName.FullName}.MemBlocks.g.cs", sourceText);
            }
        }
    }
}
