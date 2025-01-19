@echo off

::
:: converts templates to generators
::

set _cli=.\DTOMaker.CLI\bin\debug\net8.0\DTOMaker.CLI.exe

call :t2g CSPoco
call :t2g MemBlocks
call :t2g MessagePack

goto :eof

:t2g
    call %_cli% t2g -s .\Template.%1\EntityTemplate.cs -o .\DTOMaker.%1\EntityGenerator.g.cs -n DTOMaker.%1
    goto :eof
