@echo off
call :import MemBlocks MemBlocks
call :import MemBlocks MemBlocks.Tests
goto :eof

:import
echo Copying from ..\DTOMaker-%1\DTOMaker.%2\ to .\DTOMaker.%2\ ...
pause
robocopy ..\DTOMaker-%1\DTOMaker.%2\ .\DTOMaker.%2\ *.cs /mir /z
set _rc=%errorlevel%
if %_rc% EQU 0 goto :eof
pause
goto :eof