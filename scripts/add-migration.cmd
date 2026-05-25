@echo off
setlocal
set PROJECT=..\src\Logging.Jwt.Data
set STARTUP=..\src\Logging.Jwt.Web

if "%~1"=="" (
    echo Usage: add-migration.cmd MigrationName
    exit /b 1
)

dotnet ef migrations add %1 --project "%PROJECT%" --startup-project "%STARTUP%"
if errorlevel 1 exit /b 1
echo Migration '%1' added successfully.
pause
