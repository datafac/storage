# DTOMaker.Core

todo badges go here

*Warning: This is pre-release software under active development. Not for production use. Do not use if you can't tolerate breaking changes occasionally.*

This package contains attributes for defining simple data models as interfaces, and also includes types used 
at compile-time, and by generated code at runtime.

Features implemented:
- Data types: Boolean, S/Byte, U/Int16/32/64/128, Double, Single, Half, Char, Guid, Decimal
- IFreezable support
- Templates as code, template processing
- [Obsolete] members
- Fixed length arrays of above types

Features not implemented:
- Nullable types. T? can be simulated using a pair of fields (Boolean, T) and a default implementation
on the interface.
- Enum data types. An enumeration type can be simulated with a casting property and a default implementation
on the interface.
- Strings (UTF8). String types can be simulated with a byte array field and a length field and a default
implementation on the interface.

In progress:
- Json (NewtonSoft) generator
- POCO generator

Coming next:
- Json (System.Text) generator
- ProtobufNet 3.0 generator
- other model entities
- member wire names and tags
- reservation (hidden members)
- compact layout method

Coming later:
- records
- Google Protobuf .proto generation
- polymorphic types
- generic patterns: lists, trees, unions, etc.
- variable length arrays
- logical value equality

These models are consumed by the following source generators to emit DTO classes that implement the 
model interfaces.

## DTOMaker.MemBlocks
Generates DTOs whose internal data is a single memory block (Memory\<byte\>). Property getters and setters decode and encode
values directly to the block with explicit byte ordering (little-endian or big-endian). This source generator can be found 
at: https://github.com/Psiman62/DTOMaker-MemBlocks

## DTOMaker.MessagePack
Generates DTOs decorated with MessagePack attributes (https://github.com/MessagePack-CSharp/MessagePack-CSharp).
This source generator can be found at: https://github.com/Psiman62/DTOMaker.MessagePack

## Benchmarks

Some benchmarking comparing the serialization and deserialization performance of various generated DTOs can
be found at: https://github.com/Psiman62/DTOMaker-Samples