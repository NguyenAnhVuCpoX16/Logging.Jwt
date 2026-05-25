@echo off
setlocal
set PROJECT=..\src\Logging.Jwt.Data
set STARTUP=..\src\Logging.Jwt.Web

dotnet ef database update --project "%PROJECT%" --startup-project "%STARTUP%"
if errorlevel 1 exit /b 1
echo Database updated successfully.
pause
