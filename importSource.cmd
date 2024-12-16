
@echo off

::
:: Imports some generators source for test purposes
::
echo Import source?
pause

call :importgroup MemBlocks
call :importgroup MessagePack
call :importgroup CSPoco

goto :eof

:importgroup
call :import DTOMaker.%1
call :import DTOMaker.%1.Tests
call :import Template.%1
call :import Template.%1.Tests
goto :eof


:import
robocopy ..\DTOMaker\%1\ .\%1\ *.cs /mir /xd bin obj /z
set _rc=%errorlevel%
echo Robocopy returned: %_rc%
if %_rc% EQU 2 goto :eof
if %_rc% EQU 0 goto :eof
pause
goto :eof