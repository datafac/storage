using DTOMaker.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DTOMaker.Generator
{
    internal sealed class TargetEntity : TargetBase
    {
        public ConcurrentDictionary<string, TargetMember> Members { get; } = new ConcurrentDictionary<string, TargetMember>();
        public TargetEntity(string name, Location location) : base(name, location) { }
        public int BlockSize { get; set; }

        private bool BlockSizeIsValid()
        {
            return BlockSize switch
            {
                1 => true,
                2 => true,
                4 => true,
                8 => true,
                16 => true,
                32 => true,
                64 => true,
                128 => true,
                256 => true,
                512 => true,
                1024 => true,
                _ => false
            };
        }

        public bool CanEmit()
        {
            return !string.IsNullOrWhiteSpace(Name) && BlockSize > 0;
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            if(!BlockSizeIsValid())
            {
                yield return new SyntaxDiagnostic(_location, DiagnosticSeverity.Error, $"BlockSize ({BlockSize}) is invalid. BlockSize must be a power of 2, and between 1 and 1024");
            }
        }
    }
}
