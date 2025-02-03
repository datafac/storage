using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
namespace DTOMaker.Gentime
{
    // These types mimic attributes in DTOMaker.Models and must be kept in sync.
    public readonly struct DomainAttribute { }
    public readonly struct EntityAttribute { }
    public readonly struct MemberAttribute { }
    public readonly struct IdAttribute { }
}
