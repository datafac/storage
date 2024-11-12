
@echo off

::
:: Exports source back to generators projects.
::
echo Export source?
pause

call :exportgroup MemBlocks
call :exportgroup MessagePack
call :exportgroup CSPoco

goto :eof

:exportgroup
call :export %1 DTOMaker.%1
call :export %1 DTOMaker.%1.Tests
call :export %1 Template.%1
call :export %1 Template.%1.Tests
goto :eof


:export
robocopy .\%2\ ..\DTOMaker-%1\%2\ *.cs /mir /xd bin obj /z
set _rc=%errorlevel%
echo Robocopy returned: %_rc%
if %_rc% EQU 2 goto :eof
if %_rc% EQU 0 goto :eof
pause
goto :eof