
@echo off

::
:: Imports some generators source for test purposes
::

call :group MemBlocks
call :group MessagePack

goto :eof

:group
call :import %1 DTOMaker.%1
call :import %1 DTOMaker.%1.Tests
call :import %1 Template.%1
call :import %1 Template.%1.Tests
goto :eof


:import
robocopy ..\DTOMaker-%1\%2\ .\%2\ *.cs /mir /z
set _rc=%errorlevel%
echo Robocopy returned: %_rc%
if %_rc% EQU 0 goto :eof
pause
goto :eof