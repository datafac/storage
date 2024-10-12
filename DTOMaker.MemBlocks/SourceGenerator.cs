using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
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
            switch (member.MemberType)
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
                            $"Unsupported member type: '{member.MemberType}'"));
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
            foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
            {
                int fieldLength = GetFieldLength(member);
                // calculate this offset
                while (fieldLength > 0 && fieldOffset % fieldLength != 0)
                {
                    fieldOffset++;
                }
                member.FieldLength = fieldLength;
                member.FieldOffset = fieldOffset;
                // calc next offset
                fieldOffset = fieldOffset + fieldLength;
                while (fieldOffset > minBlockLength)
                {
                    minBlockLength = minBlockLength == 0 ? 1 : minBlockLength * 2;
                }
                // todo allocate Flags byte
            }
            entity.BlockLength = minBlockLength;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver syntaxReceiver) return;

            // check that the users compilation references the expected libraries
            CheckReferencedAssemblyNamesInclude(context, typeof(DTOMaker.Runtime.IFieldCodec).Assembly);

            foreach (var domain in syntaxReceiver.Domains.Values)
            {
                EmitDiagnostics(context, domain);
                foreach (var entity in domain.Entities.Values.OrderBy(e => e.Name))
                {
                    // do any auto-layout if required
                    AutoLayoutMembers(entity);

                    // run checks
                    EmitDiagnostics(context, entity);

                    Version fv = new Version(ThisAssembly.AssemblyFileVersion);
                    string shortVersion = $"{fv.Major}.{fv.Minor}";
                    string hintName = $"{domain.Name}.{entity.Name}.MemBlocks.g.cs";
                    // entity options
                    var builder = new StringBuilder();
                    string entityHead =
                        $$"""
                        // <auto-generated>
                        // This file was generated by {{typeof(SourceGenerator).Namespace}}.
                        // NuGet: https://www.nuget.org/packages/DTOMaker.MemBlocks
                        // Warning: Changes made to this file will be lost if re-generated.
                        // </auto-generated>
                        #pragma warning disable CS0414
                        #nullable enable
                        using DTOMaker.Runtime;
                        using System;
                        using System.Runtime.CompilerServices;
                        using System.Threading;
                        using System.Threading.Tasks;
                        namespace {{domain.Name}}.MemBlocks
                        {
                            public partial class {{entity.Name}} : I{{entity.Name}}, IFreezable
                            {
                                private const int BlockLength = {{entity.BlockLength}};
                                private readonly Memory<byte> _writableBlock;
                                private readonly ReadOnlyMemory<byte> _readonlyBlock;
                                public ReadOnlyMemory<byte> Block => _frozen ? _readonlyBlock : _writableBlock.ToArray();

                                public {{entity.Name}}() => _readonlyBlock = _writableBlock = new byte[BlockLength];
                       
                                public {{entity.Name}}(ReadOnlySpan<byte> source, bool frozen)
                                {
                                    Memory<byte> memory = new byte[BlockLength];
                                    source.Slice(0, BlockLength).CopyTo(memory.Span);
                                    _readonlyBlock = memory;
                                    _writableBlock = memory;
                                    _frozen = frozen;
                                }
                                public {{entity.Name}}(ReadOnlyMemory<byte> source)
                                {
                                    if (source.Length >= BlockLength)
                                    {
                                        _readonlyBlock = source.Slice(0, BlockLength);
                                    }
                                    else
                                    {
                                        // forced copy as source is too short
                                        Memory<byte> memory = new byte[BlockLength];
                                        source.Slice(0, BlockLength).Span.CopyTo(memory.Span);
                                        _readonlyBlock = memory;
                                    }
                                    _writableBlock = Memory<byte>.Empty;
                                    _frozen = true;
                                }
                                // todo move to base
                                private volatile bool _frozen = false;
                                public bool IsFrozen() => _frozen;
                                public IFreezable PartCopy() => new {{entity.Name}}(this);

                                [MethodImpl(MethodImplOptions.NoInlining)]
                                private void ThrowIsFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} when frozen.");

                                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                                private ref T IfNotFrozen<T>(ref T value, [CallerMemberName] string? methodName = null)
                                {
                                    if (_frozen) ThrowIsFrozenException(methodName);
                                    return ref value;
                                }

                                public void Freeze()
                                {
                                    if (_frozen) return;
                                    _frozen = true;
                                    // todo freeze base
                                    // todo freeze model type refs
                                }
                        
                                public {{entity.Name}}(I{{entity.Name}} source) : this(ReadOnlySpan<byte>.Empty, false)
                                {
                                    // todo base ctor
                                    // todo freezable members
                        """;
                    builder.AppendLine(entityHead);
                    foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
                    {
                        string memberPart1 =
                            // 12sp
                            $$"""
                                        this.{{member.Name}} = source.{{member.Name}};
                            """;
                        builder.AppendLine(memberPart1);
                    }
                    string entityPart1 =
                        // 8sp
                        """
                                }

                        """;
                    builder.AppendLine(entityPart1);
                    // begin member map
                    string memberMapHead =
                        """
                                // <field-map>
                                //  Seq.  Off.  Len.  Type        Endian  Name
                                //  ----  ----  ----  --------    ------  --------
                        """;
                    builder.AppendLine(memberMapHead);
                    foreach (var member in entity.Members.Values.OrderBy(m => m.FieldOffset))
                    {
                        string memberMapBody =
                            $$"""
                                    //  {{member.Sequence,4:N0}}  {{member.FieldOffset,4:N0}}  {{member.FieldLength,4:N0}}  {{member.MemberType,-8}}    {{(member.IsBigEndian ? "Big   " : "Little")}}  {{member.Name}}
                            """;
                        builder.AppendLine(memberMapBody);
                    }
                    string memberMapTail =
                        """
                                // </field-map>
                        """;
                    builder.AppendLine(memberMapTail);
                    // end member map
                    // begin member def
                    foreach (var member in entity.Members.Values.OfType<MemBlockMember>().OrderBy(m => m.FieldOffset))
                    {
                        EmitDiagnostics(context, member);
                        string memberDefBody =
                            $$"""
                                    public {{member.MemberType}} {{member.Name}}
                                    {
                                        get => {{member.CodecTypeName}}.ReadFromSpan(_readonlyBlock.Slice({{member.FieldOffset}}, {{member.FieldLength}}).Span);
                                        set => {{member.CodecTypeName}}.WriteToSpan(_writableBlock.Slice({{member.FieldOffset}}, {{member.FieldLength}}).Span, IfNotFrozen(ref value));
                                    }

                            """;
                        builder.AppendLine(memberDefBody);
                    }
                    // end member def
                    string entityTail =
                        """
                            }
                        }
                        """;
                    builder.AppendLine(entityTail);
                    context.AddSource(hintName, builder.ToString());
                }
            }
        }
    }
}
