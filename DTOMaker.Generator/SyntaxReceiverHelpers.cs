using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace DTOMaker.Generator
{
    internal static class SyntaxReceiverHelpers
    {
        public static bool IsIdentifierForAttributeName(this IdentifierNameSyntax ins, string attributeName)
        {
            var prefix = ins.Identifier.Text.AsSpan();
            var suffix = nameof(Attribute).AsSpan();
            var candidate = attributeName.AsSpan();
            return candidate.Length == (prefix.Length + suffix.Length)
                && candidate.StartsWith(prefix)
                && candidate.EndsWith(suffix);
        }

        public static bool HasOneAttributeNamed(this InterfaceDeclarationSyntax ids, string attributeName)
        {
            var allAttributes = ids.AttributeLists.SelectMany(al => al.Attributes).ToArray();
            if (allAttributes.Length != 1) return false;

            return allAttributes[0].Name is IdentifierNameSyntax ins && ins.IsIdentifierForAttributeName(attributeName);
        }

        public static bool HasOneAttributeNamed(this ClassDeclarationSyntax cds, string attributeName)
        {
            var allAttributes = cds.AttributeLists.SelectMany(al => al.Attributes).ToArray();
            if (allAttributes.Length != 1) return false;

            return allAttributes[0].Name is IdentifierNameSyntax ins && ins.IsIdentifierForAttributeName(attributeName);
        }

        public static bool HasOneAttributeNamed(this PropertyDeclarationSyntax pds, string attributeName)
        {
            var allAttributes = pds.AttributeLists.SelectMany(al => al.Attributes).ToArray();
            if (allAttributes.Length != 1) return false;

            return allAttributes[0].Name is IdentifierNameSyntax ins && ins.IsIdentifierForAttributeName(attributeName);
        }
    }
}
