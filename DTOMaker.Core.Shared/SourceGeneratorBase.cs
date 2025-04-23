using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
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

        private static string MakeClosedFullName(TypeFullName openTFN, ImmutableArray<ITypeParameterSymbol> typeParameters, ImmutableArray<ITypeSymbol> typeArguments)
        {
            string result = openTFN.FullName;
            int length = Math.Min(typeParameters.Length, typeArguments.Length);
            for (int i = 0; i < length; i++)
            {
                string pattern = TypeFullName.Create(typeParameters[i]).ShortImplName;
                string replace = TypeFullName.Create(typeArguments[i]).ShortImplName;
                result = result.Replace(pattern, replace);
            }
            return result;
        }

        private static TypeFullName ResolveMemberType(TargetDomain domain, TypeFullName closedEntityTFN, TargetMember openMember)
        {
            // search for direct open/closed argument match
            for (int i = 0; i < closedEntityTFN.TypeParameters.Length; i++)
            {
                TypeFullName openMemberTFN = TypeFullName.Create(closedEntityTFN.TypeParameters[i]);
                if (openMember.MemberType == openMemberTFN)
                {
                    var mTFN = TypeFullName.Create(closedEntityTFN.TypeArguments[i]);
                    return mTFN;
                }
            }

            // search closed entities for match
            string candidateFullName = MakeClosedFullName(openMember.MemberType, closedEntityTFN.TypeParameters, closedEntityTFN.TypeArguments);
            foreach (var closedEntity in domain.ClosedEntities.Values)
            {
                if (string.Equals(candidateFullName, closedEntity.TFN.FullName))
                {
                    return closedEntity.TFN;
                }
            }
            // oops - not resolved
            return openMember.MemberType;
        }

        protected abstract void OnExecute(GeneratorExecutionContext context);
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiverBase syntaxReceiver) return;

            // fix entity hierarchy
            var domain = syntaxReceiver.Domain;
            var entities = domain.ClosedEntities.Values.ToArray();
            foreach (var entity in entities)
            {
                if (!entity.BaseName.Equals(TypeFullName.DefaultBase))
                {
                    if (domain.ClosedEntities.TryGetValue(entity.BaseName.FullName, out var baseEntity))
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
                if (entity.TFN.IsGeneric && entity.TFN.IsClosed)
                {
                    var openTFN = entity.TFN.AsOpenGeneric();
                    if (domain.OpenEntities.TryGetValue(openTFN.FullName, out var openEntity))
                    {
                        // generate id and members if required
                        if (entity.OpenEntity is null)
                        {
                            entity.OpenEntity = openEntity;
                            entity.HasEntityAttribute = true; // implied
                            // generate id
                            SyntheticId syntheticId = new SyntheticId(openEntity.EntityId);
                            foreach (var ta in entity.TFN.TypeArguments)
                            {
                                syntheticId = syntheticId.Add(TypeFullName.Create(ta).SyntheticId);
                            }
                            entity.EntityId = syntheticId.Id;
                            // generate members
                            foreach (TargetMember openMember in openEntity.Members.Values)
                            {
                                TargetMember member = syntaxReceiver.Factory.CloneMember(entity, openMember);
                                if(member.Kind == MemberKind.Unknown)
                                {
                                    var mTFN = ResolveMemberType(domain, entity.TFN, openMember);
                                    member.MemberType = mTFN;
                                    member.Kind = mTFN.MemberKind;
                                    if (mTFN.MemberKind == MemberKind.Unknown && domain.ClosedEntities.TryGetValue(mTFN.FullName, out var _))
                                    {
                                        member.Kind = MemberKind.Entity;
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
                                $"Cannot find open entity '{openTFN}' for closed entity '{entity.TFN}'."));
                    }
                }
            }

            // determine derived entities
            foreach (var entity in entities)
            {
                entity.DerivedEntities = domain.ClosedEntities.Values
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
