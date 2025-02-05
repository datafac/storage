# DTOMaker

Supports model-first development of DTOs (Data Transport Objects), DSOs (Data Storage 
Objects) and related POCOs (Plain Old C# Objects). Models are defined as C# interfaces 
with additional attributes.

[![Build-Deploy](https://github.com/datafac/dtomaker-core/actions/workflows/dotnet.yml/badge.svg)](https://github.com/datafac/dtomaker-core/actions/workflows/dotnet.yml)

*Warning: This is pre-release software under active development. Breaking changes may occur.*

This repo includes the following packages:

## DTOMaker.Models
Attributes for defining simple data models as interfaces in C#.

## DTOMaker.MessagePack
A source generator that creates MessagePack DTOs (Data Transport Objects).
See https://github.com/MessagePack-CSharp/MessagePack-CSharp.
## DTOMaker.Models.MessagePack
Additional attributes used by the DTOMaker.MessagePack source generator, to manage
MessagePack entity and member keys.

## DTOMaker.MemBlocks
Generates DTOs whose internal data is a single memory block (Memory\<byte\>). Property getters and setters decode and encode
values directly to the block with explicit byte ordering (little-endian or big-endian).
## DTOMaker.Models.MemBlocks
Additional attributes used by the DTOMaker.MemBlocks source generator, to 
manage entity and member memory layout.

## DTOMaker.CSPoco
Generates basic POCOs (Plain Old C# Objects) that implement the model interfaces.

## DTOMaker.Runtime
Common types used at runtime by DTOMaker generated entities.

## Model Features
- Member value types: Boolean, S/Byte, U/Int16/32/64/128, Double, Single, Half, Char, Guid, Decimal
- String member types:
  - Required or nullable strings for CSPoco and MessagePack
  - Non-nullable UTF8-encoded fixed length strings for MemBlocks
- Variable length binary member types:
  - Octets (model interfaces, CSPoco, MemBlocks)
  - ReadOnlyMemory\<byte\> (MessagePack)
- IFreezable support
- Templates as testable code, template-to-generator processing
- [Obsolete] members
- Fixed length arrays of above value types.
- polymorphic types
- entity members
- IEquatable\<T\> support

## Limitations
- single assembly contains models and generated DTOs 

## In progress:
- single block allocation (MemBlocks)
- variable-length string members

## Coming soon:
- global interface equality comparer
- auto-embedded entity members (MemBlocks)
- auto-embedded binary members (MemBlocks)
- auto-embedded string members (MemBlocks)
- fixed-length binary members (MemBlocks)
- fixed-length string members (MemBlocks)
- reservation (hidden members)
- Json (NewtonSoft) generator
- Json (System.Text) generator
- Orleans generator
- ProtobufNet 3.0 generator
- MessagePack 3.x generator
- NetStrux generator
- MemBlocks compact layout method
- Enum data types. Workaround - enums can be implemented with an underlying 
  integer property and a cast.
- MemBlocks nullable types. Workaround - T? can be implemented with a pair
  of members (Boolean, T).

## Coming later:
- custom struct members (to avoid primitive obsession)
- C# records generator
- Google Protobuf .proto generation
- model.json generation
- command-line alternative
- generic model patterns: lists, trees, unions, etc.
- variable length arrays
- logical value equality
- Rune member types
