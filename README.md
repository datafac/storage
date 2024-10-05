# DTOMaker.Core

todo badges go here

*Warning: This is pre-release software under active development. Not for production use. Do not use if you can't tolerate breaking changes occasionally.*

This package contains attributes for defining simple data models as interfaces, and also includes types used 
at compile-time, and by generated code at runtime.

Features implemented:
- Member data types: Boolean, S/Byte, U/Int16/32/64, Double, Single, Char

Yet to be implemented:
- other member types: Half, Decimal, Guid, fixed length byte/char arrays
- model interface support
- mutable/immutable/freezable support
- enumerations
- records
- nullable types
- other model types
- Google Protobuf .proto generation
- polymorphic types
- fixed length arrays
- generic patterns: lists, trees, unions, etc.
- variable length arrays

These models are consumed by the following source generators to emit DTO classes that implement the 
model interfaces.

## DTOMaker.MemBlocks
Generates DTOs whose internal data is a single memory block (Memory\<byte\>). Property getters and setters decode and encode
values directly to the block with explicit byte ordering (little-endian or big-endian). This source generator can be found 
at: https://github.com/Psiman62/DTOMaker-MemBlocks

## DTOMaker.MessagePack
Generates DTOs decorated with MessagePack attributes (https://github.com/MessagePack-CSharp/MessagePack-CSharp).
This source generator can be found at: https://github.com/Psiman62/DTOMaker.MessagePack

## DTOMaker.JsonNewtonSoft (soon)

