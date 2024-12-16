
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
call :export DTOMaker.%1
call :export DTOMaker.%1.Tests
call :export Template.%1
call :export Template.%1.Tests
goto :eof


:export
robocopy .\%1\ ..\DTOMaker\%1\ *.cs /mir /xd bin obj /z
set _rc=%errorlevel%
echo Robocopy returned: %_rc%
if %_rc% EQU 2 goto :eof
if %_rc% EQU 0 goto :eof
pause
goto :eof