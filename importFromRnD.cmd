: 
: imports source from RnD repo
:
@echo off

call :import DataFac.Storage
call :import DataFac.Storage.LocalFS
call :import DataFac.Storage.RocksDb
call :import DataFac.Storage.Testing
call :import DataFac.Storage.Tests

goto :eof

:import csFromDir
    echo Importing from %1 ...
    robocopy ..\rnd\%1 .\%1 *.cs /mir /xx /z
    set _rc=%errorlevel%
    echo     Returned: %_rc%
    goto :eof
