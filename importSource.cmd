
@echo off

::
:: Imports some generators source for test purposes
::

:: call :import MessagePack MessagePack
:: call :import MessagePack MessagePack.Tests

call :import MemBlocks MemBlocks
call :import MemBlocks MemBlocks.Tests

goto :eof

:import
robocopy ..\DTOMaker-%1\DTOMaker.%2\ .\DTOMaker.%2\ *.cs /mir /z
set _rc=%errorlevel%
echo Robocopy returned: %_rc%
if %_rc% EQU 0 goto :eof
pause
goto :eof