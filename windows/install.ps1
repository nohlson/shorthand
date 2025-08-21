# Shorthand Windows Installer
# This script installs and configures Shorthand for Windows

param(
    [switch]$SkipOllamaCheck,
    [switch]$SkipModelSetup
)

Write-Host "Shorthand Windows Installer" -ForegroundColor Green
Write-Host "==========================" -ForegroundColor Green
Write-Host ""

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "This script should be run as Administrator for best results."
    Write-Host "Press any key to continue anyway..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

# Check if .NET 8.0 is installed
Write-Host "Checking .NET 8.0 installation..." -ForegroundColor Cyan
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -like "8.*") {
        Write-Host "✓ .NET 8.0 is installed: $dotnetVersion" -ForegroundColor Green
    } else {
        Write-Host "✗ .NET 8.0 is required but found: $dotnetVersion" -ForegroundColor Red
        Write-Host "Please install .NET 8.0 from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "✗ .NET 8.0 is not installed" -ForegroundColor Red
    Write-Host "Please install .NET 8.0 from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

# Check if Ollama is installed and running
if (-not $SkipOllamaCheck) {
    Write-Host "Checking Ollama installation..." -ForegroundColor Cyan
    
    # Check if Ollama is installed
    $ollamaPath = Get-Command ollama -ErrorAction SilentlyContinue
    if (-not $ollamaPath) {
        Write-Host "✗ Ollama is not installed" -ForegroundColor Red
        Write-Host "Please install Ollama from: https://ollama.com/download" -ForegroundColor Yellow
        Write-Host "After installation, run this script again." -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "✓ Ollama is installed" -ForegroundColor Green
    
    # Check if Ollama is running
    try {
        $response = Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/tags" -Method Get -TimeoutSec 5
        Write-Host "✓ Ollama is running" -ForegroundColor Green
    } catch {
        Write-Host "✗ Ollama is not running" -ForegroundColor Red
        Write-Host "Starting Ollama..." -ForegroundColor Yellow
        
        try {
            Start-Process -FilePath "ollama" -ArgumentList "serve" -WindowStyle Hidden
            Start-Sleep -Seconds 3
            
            $response = Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/tags" -Method Get -TimeoutSec 5
            Write-Host "✓ Ollama started successfully" -ForegroundColor Green
        } catch {
            Write-Host "✗ Failed to start Ollama" -ForegroundColor Red
            Write-Host "Please start Ollama manually and run this script again." -ForegroundColor Yellow
            exit 1
        }
    }
}

# Build the application
Write-Host "Building Shorthand application..." -ForegroundColor Cyan
try {
    Set-Location $PSScriptRoot
    dotnet build -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Build failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Build successful" -ForegroundColor Green
} catch {
    Write-Host "✗ Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Create installation directory
$installDir = "$env:LOCALAPPDATA\Shorthand"
Write-Host "Installing to: $installDir" -ForegroundColor Cyan

if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

# Copy application files
$binDir = "$installDir\bin"
if (-not (Test-Path $binDir)) {
    New-Item -ItemType Directory -Path $binDir -Force | Out-Null
}

Copy-Item -Path "bin\Release\net8.0-windows\*" -Destination $binDir -Recurse -Force
Copy-Item -Path "Modelfile" -Destination $installDir -Force

# Create desktop shortcut
$desktopPath = [Environment]::GetFolderPath("Desktop")
$shortcutPath = "$desktopPath\Shorthand.lnk"
$targetPath = "$binDir\Shorthand.Windows.exe"

$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($shortcutPath)
$Shortcut.TargetPath = $targetPath
$Shortcut.WorkingDirectory = $binDir
$Shortcut.Description = "Shorthand - Local-only prompt-to-command generator"
$Shortcut.Save()

Write-Host "✓ Desktop shortcut created" -ForegroundColor Green

# Create start menu shortcut
$startMenuPath = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Shorthand"
if (-not (Test-Path $startMenuPath)) {
    New-Item -ItemType Directory -Path $startMenuPath -Force | Out-Null
}

$startMenuShortcut = "$startMenuPath\Shorthand.lnk"
$Shortcut = $WshShell.CreateShortcut($startMenuShortcut)
$Shortcut.TargetPath = $targetPath
$Shortcut.WorkingDirectory = $binDir
$Shortcut.Description = "Shorthand - Local-only prompt-to-command generator"
$Shortcut.Save()

Write-Host "✓ Start menu shortcut created" -ForegroundColor Green

# Setup Ollama model if not skipped
if (-not $SkipModelSetup) {
    Write-Host "Setting up Ollama model..." -ForegroundColor Cyan
    
    try {
        # Check if base model exists
        $models = Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/tags" -Method Get
        $baseModelExists = $models.models | Where-Object { $_.name -eq "llama3.2:3b" }
        
        if (-not $baseModelExists) {
            Write-Host "Pulling base model llama3.2:3b..." -ForegroundColor Yellow
            Start-Process -FilePath "ollama" -ArgumentList "pull", "llama3.2:3b" -Wait -NoNewWindow
        }
        
        # Create cmdgen model
        $cmdgenExists = $models.models | Where-Object { $_.name -eq "cmdgen" }
        if (-not $cmdgenExists) {
            Write-Host "Creating cmdgen model..." -ForegroundColor Yellow
            Start-Process -FilePath "ollama" -ArgumentList "create", "cmdgen", "-f", "$installDir\Modelfile" -Wait -NoNewWindow
        }
        
        Write-Host "✓ Ollama model setup complete" -ForegroundColor Green
    } catch {
        Write-Host "⚠ Ollama model setup failed: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "You can run 'ollama create cmdgen -f $installDir\Modelfile' manually later" -ForegroundColor Yellow
    }
}

# Create configuration file
$configPath = "$installDir\config.json"
if (-not (Test-Path $configPath)) {
    $defaultConfig = @{
        TerminalApplications = @{
            "mobaxterm" = @{
                PasteMethod = "ShiftInsert"
                ProcessNames = @("mobaxterm")
            }
            "putty" = @{
                PasteMethod = "RightClick"
                ProcessNames = @("putty")
            }
            "windows_terminal" = @{
                PasteMethod = "CtrlV"
                ProcessNames = @("WindowsTerminal", "wt")
            }
            "powershell" = @{
                PasteMethod = "CtrlV"
                ProcessNames = @("powershell", "pwsh")
            }
            "cmd" = @{
                PasteMethod = "CtrlV"
                ProcessNames = @("cmd", "conhost")
            }
        }
        OllamaUrl = "http://127.0.0.1:11434"
        ModelName = "cmdgen"
        Hotkey = "Ctrl+G"
        ShowCommandPreview = $true
        RequireConfirmation = $true
    }
    
    $defaultConfig | ConvertTo-Json -Depth 10 | Out-File -FilePath $configPath -Encoding UTF8
    Write-Host "✓ Configuration file created" -ForegroundColor Green
}

Write-Host ""
Write-Host "Installation complete!" -ForegroundColor Green
Write-Host "=====================" -ForegroundColor Green
Write-Host ""
Write-Host "To use Shorthand:" -ForegroundColor Cyan
Write-Host "1. Double-click the Shorthand shortcut on your desktop" -ForegroundColor White
Write-Host "2. The application will run in the system tray" -ForegroundColor White
Write-Host "3. Press Ctrl+G in any terminal application to generate commands" -ForegroundColor White
Write-Host "4. Right-click the tray icon to access settings" -ForegroundColor White
Write-Host ""
Write-Host "Supported terminal applications:" -ForegroundColor Cyan
Write-Host "- MobaXterm (Shift+Insert paste)" -ForegroundColor White
Write-Host "- PuTTY (Right-click paste)" -ForegroundColor White
Write-Host "- Windows Terminal (Ctrl+V paste)" -ForegroundColor White
Write-Host "- PowerShell (Ctrl+V paste)" -ForegroundColor White
Write-Host "- Command Prompt (Ctrl+V paste)" -ForegroundColor White
Write-Host ""
Write-Host "Configuration file: $configPath" -ForegroundColor Gray
Write-Host "Application directory: $binDir" -ForegroundColor Gray
