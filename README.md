# DTOMaker

Types and source generators for building DTOs (Data Transport Objects) 
and related POCOs (Plain Old C# Objects).

todo build badge

*Warning: This is pre-release software under active development. Breaking changes may occur.*

## DTOMaker.Models
Attributes for defining simple data models as interfaces in C#.

## DTOMaker.Models.MemBlocks
Additional attributes used by the DTOMaker.MemBlocks source generator, to 
manage entity and member memory layout.

## DTOMaker.Models.MessagePack
Additional attributes used by the DTOMaker.MessagePack source generator, to manage
MessagePack entity and member keys.

## DTOMaker.Core
Core types used by source generators at compile-time, including a common syntax receiver, and a powerful template processor.

## Features

Features implemented:
- DTOMaker.MessagePack source generator
- DTOMaker.MemBlocks source generator
- DTOMaker.CSPoco source generator
- Member value types: Boolean, S/Byte, U/Int16/32/64/128, Double, Single, Half, Char, Guid, Decimal
- String member types:
  - Required or nullable strings for CSPoco and MessagePack
  - Non-nullable UTF8-encoded fixed length strings for MemBlocks
- IFreezable support
- Templates as code, template processing
- [Obsolete] members
- Fixed length arrays of above value types.
- polymorphic types

Features not implemented:
- Nullable types. T? can be implemented with a pair of fields (Boolean, T).
- Enum data types. Enums can be implemented with an underlying integer property and a cast.

In progress:
- multi-namespace models
- entity members

Coming soon:
- reservation (hidden members)
- Json (NewtonSoft) generator
- Json (System.Text) generator
- Orleans generator
- ProtobufNet 3.0 generator
- NetStrux generator
- compact layout method

Coming later:
- C# records generator
- Google Protobuf .proto generation
- model.json generation
- command-line alternative
- generic patterns: lists, trees, unions, etc.
- variable length arrays
- logical value equality

These models are consumed by the following source generators to emit DTO classes that implement the 
model interfaces.

## DTOMaker.Runtime
Common types used at runtime by DTOMaker entities.

## Benchmarks

Some benchmarking comparing the serialization and deserialization performance of various generated DTOs can
be found at: todo