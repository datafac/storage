::
:: converts templates to generators
::

set _cli=.\DTOMaker.CLI\bin\debug\net8.0\DTOMaker.CLI.exe

call %_cli% t2g -s .\Template.MessagePack\EntityTemplate.cs -o .\DTOMaker.MessagePack\EntityGenerator.g.cs -n DTOMaker.MessagePack

