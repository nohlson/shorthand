@echo off
echo Shorthand Windows Installer
echo ==========================
echo.
echo This will launch the PowerShell installer.
echo Make sure you have .NET 8.0 and Ollama installed.
echo.
pause

powershell -ExecutionPolicy Bypass -File "%~dp0install.ps1"

echo.
echo Installation complete! Press any key to exit.
pause >nul
