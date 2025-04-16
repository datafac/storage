using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace DTOMaker.Gentime
{

    public abstract class SourceGeneratorBase : ISourceGenerator
    {
        protected abstract void OnInitialize(GeneratorInitializationContext context);
        public void Initialize(GeneratorInitializationContext context) => OnInitialize(context);

        private static bool IsDerivedFrom(TargetEntity candidate, TargetEntity parent)
        {
            if (ReferenceEquals(candidate, parent)) return false;
            if (candidate.Base is null) return false;
            if (candidate.Base.TFN.Equals(parent.TFN)) return true;
            return IsDerivedFrom(candidate.Base, parent);
        }

        private static int GetSyntheticId(TypeFullName tfn)
        {
            return tfn.FullName switch
            {
                FullTypeName.SystemBoolean => 9001,
                FullTypeName.SystemSByte => 9002,
                FullTypeName.SystemByte => 9003,
                FullTypeName.SystemInt16 => 9004,
                FullTypeName.SystemUInt16 => 9005,
                FullTypeName.SystemChar => 9006,
                FullTypeName.SystemHalf => 9007,
                FullTypeName.SystemInt32 => 9008,
                FullTypeName.SystemUInt32 => 9009,
                FullTypeName.SystemSingle => 9010,
                FullTypeName.SystemInt64 => 9011,
                FullTypeName.SystemUInt64 => 9012,
                FullTypeName.SystemDouble => 9013,
                FullTypeName.SystemString => 9014,
                FullTypeName.MemoryOctets => 9099,
                _ => throw new NotSupportedException($"Cannot synthesize id for type '{tfn}'"),
            };
        }

        protected abstract void OnExecute(GeneratorExecutionContext context);
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiverBase syntaxReceiver) return;

            // fix entity hierarchy
            var domain = syntaxReceiver.Domain;
            var entities = domain.Entities.Values.ToArray();
            foreach (var entity in entities)
            {
                if (!entity.BaseName.Equals(TypeFullName.DefaultBase))
                {
                    if (domain.Entities.TryGetValue(entity.BaseName.FullName, out var baseEntity))
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

            // bind closed/open generic entities
            foreach (var entity in entities)
            {
                var eTFN = entity.TFN;
                if (eTFN.IsGeneric && eTFN.IsClosed)
                {
                    var openTFN = eTFN.AsOpenGeneric();
                    if (domain.Entities.TryGetValue(openTFN.FullName, out var openEntity))
                    {
                        // generate id and members if required
                        if (entity.OpenEntity is null)
                        {
                            entity.OpenEntity = openEntity;
                            entity.HasEntityAttribute = true; // implied
                            // generate id
                            SyntheticId syntheticId = new SyntheticId(openEntity.EntityId);
                            foreach (var ta in eTFN.TypeArguments)
                            {
                                syntheticId = syntheticId.Add(GetSyntheticId(TypeFullName.Create(ta)));
                            }
                            entity.EntityId = syntheticId.Id;
                            // generate members
                            foreach (TargetMember openMember in openEntity.Members.Values)
                            {
                                TargetMember member = syntaxReceiver.Factory.CloneMember(entity, openMember);
                                for (int i = 0; i < eTFN.TypeParameters.Length; i++)
                                {
                                    TypeFullName openMemberTFN = TypeFullName.Create(eTFN.TypeParameters[i]);
                                    if (openMember.MemberType == openMemberTFN)
                                    {
                                        member.MemberType = TypeFullName.Create(eTFN.TypeArguments[i]);
                                    }
                                }
                                entity.Members.TryAdd(member.Name, member);
                            }
                        }
                    }
                    else
                    {
                        // open entity not found!
                        entity.SyntaxErrors.Add(
                            new SyntaxDiagnostic(
                                DiagnosticId.DTOM0011, "Invalid generic entity", DiagnosticCategory.Design, entity.Location, DiagnosticSeverity.Error,
                                $"Cannot find open entity '{openTFN}' for closed entity '{eTFN}'."));
                    }
                }
            }

            // determine derived entities
            foreach (var entity in entities)
            {
                entity.DerivedEntities = domain.Entities.Values
                    .Where(e => IsDerivedFrom(e, entity))
                    .OrderBy(e => e.TFN.FullName)
                    .ToArray();
            }

            // determine entity members
            foreach (var entity in entities)
            {
                foreach (var member in entity.Members.Values)
                {
                    var entity2 = entities.FirstOrDefault(e => e.TFN == member.MemberType);
                    if (entity2 is not null)
                    {
                        member.Kind = MemberKind.Entity;
                    }
                }
            }

            // todo emit metadata as json
            //var metadata = new JsonModel();
            //metadata.Entities = entities
            //    .OrderBy(e => e.EntityId)
            //    .Select(e => e.ToJson())
            //    .ToArray();
            //string jsonText = metadata.ToText();
            //// todo emit json file directly to file system
            //context.AddSource($"Metadata.g.json", jsonText);

            OnExecute(context);
        }
    }
}
