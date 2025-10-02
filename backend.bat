@echo off
echo Starting Mutual Fund Scanner Backend...
echo.

REM Change to the root directory (where solution file is located)
cd /d "%~dp0"

REM Restore NuGet packages
echo Restoring NuGet packages...
dotnet restore

REM Build the solution
echo Building solution...
dotnet build --no-restore

if %ERRORLEVEL% NEQ 0 (
    echo Build failed! Please check the error messages above.
    pause
    exit /b 1
)

echo.
echo Build successful! Starting the API server...
echo.
echo API will be available at:
echo   HTTP:  http://localhost:5247
echo   HTTPS: https://localhost:7052
echo.
echo Press Ctrl+C to stop the server
echo.

REM Run the API project
dotnet run --project backend\MutualFundScanner.API --no-build

pause
