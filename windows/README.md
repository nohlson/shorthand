# Shorthand for Windows

Local-only prompt-to-command generator for Windows that works with any SSH client or terminal emulator.

## Features

- **Global Hotkey**: Press `Ctrl+G` from any terminal application to generate commands
- **Terminal Detection**: Automatically detects common terminal applications (MobaXterm, PuTTY, Windows Terminal, PowerShell, CMD)
- **Smart Pasting**: Configurable paste methods for different terminal applications
- **Safety First**: Command preview and confirmation before execution
- **Local Only**: All processing happens locally via Ollama - no data sent to external services

## Prerequisites

1. **Windows 10/11** (64-bit)
2. **.NET 8.0 Runtime** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
3. **Ollama** - [Download here](https://ollama.com/download)

## Quick Install

### Option 1: Automated Install (Recommended)

1. **Clone this repository**:
   ```powershell
   git clone https://github.com/nohlson/shorthand.git
   cd shorthand/windows
   ```

2. **Run the installer** (as Administrator):
   ```powershell
   .\install.ps1
   ```

The installer will:
- Check prerequisites
- Build the application
- Install to `%LOCALAPPDATA%\Shorthand`
- Create desktop and start menu shortcuts
- Set up the Ollama model
- Configure default settings

### Option 2: Manual Install

1. **Build the application**:
   ```powershell
   dotnet build -c Release
   ```

2. **Copy files** to your preferred location

3. **Set up Ollama model**:
   ```powershell
   ollama create cmdgen -f Modelfile
   ```

## Usage

### Basic Usage

1. **Start Shorthand**: Double-click the desktop shortcut or run from start menu
2. **Focus a terminal**: Open your preferred SSH client (MobaXterm, PuTTY, etc.)
3. **Generate commands**: Press `Ctrl+G` and describe what you want to do
4. **Review and execute**: Preview the generated command and confirm execution

### Example Prompts

- "list all files in current directory"
- "find files modified in the last 24 hours"
- "check disk usage for all mounted filesystems"
- "show running processes sorted by memory usage"

### Supported Terminal Applications

| Application | Default Paste Method | Notes |
|-------------|---------------------|-------|
| **MobaXterm** | Shift+Insert | Most SSH clients use this |
| **PuTTY** | Right-click | Right-click in terminal window |
| **Windows Terminal** | Ctrl+V | Standard Windows paste |
| **PowerShell** | Ctrl+V | Standard Windows paste |
| **Command Prompt** | Ctrl+V | Standard Windows paste |

## Configuration

### Access Settings

Right-click the system tray icon and select "Settings" to configure:

- **Hotkey**: Change the global hotkey (default: Ctrl+G)
- **Ollama URL**: Customize Ollama server location
- **Model Name**: Specify which Ollama model to use
- **Terminal Apps**: Add/remove terminal applications and configure paste methods
- **Safety Options**: Enable/disable command preview and confirmation

### Configuration File

Settings are stored in: `%LOCALAPPDATA%\Shorthand\config.json`

```json
{
  "TerminalApplications": {
    "mobaxterm": {
      "PasteMethod": "ShiftInsert",
      "ProcessNames": ["mobaxterm"]
    }
  },
  "OllamaUrl": "http://127.0.0.1:11434",
  "ModelName": "cmdgen",
  "Hotkey": "Ctrl+G",
  "ShowCommandPreview": true,
  "RequireConfirmation": true
}
```

### Custom Terminal Applications

To add support for a new terminal application:

1. Open Settings
2. Click "Add Terminal"
3. Enter the application name
4. Select the appropriate paste method
5. Add process names (comma-separated)

## Troubleshooting

### Common Issues

**"No terminal window detected"**
- Ensure the terminal application is focused/active
- Check if the application is in the supported terminals list
- Verify process names in settings

**"Ollama API not responding"**
- Ensure Ollama is running: `ollama serve`
- Check if Ollama is accessible at `http://127.0.0.1:11434`
- Verify the model exists: `ollama list`

**Hotkey not working**
- Check if another application is using the same hotkey
- Try changing the hotkey in settings
- Ensure Shorthand is running (check system tray)

**Commands not pasting correctly**
- Verify the paste method setting for your terminal
- Some terminals may require specific paste methods
- Test with different paste methods in settings

### Debug Mode

Enable debug logging by setting the environment variable:
```powershell
$env:SHORTHAND_DEBUG = "1"
```

### Reset Configuration

Delete the configuration file to restore defaults:
```powershell
Remove-Item "$env:LOCALAPPDATA\Shorthand\config.json"
```

## Development

### Building from Source

```powershell
# Clone repository
git clone https://github.com/nohlson/shorthand.git
cd shorthand/windows

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

### Project Structure

- `Program.cs` - Application entry point
- `MainForm.cs` - Main application form and tray icon
- `GlobalHotkey.cs` - Global hotkey registration
- `OllamaService.cs` - Communication with Ollama
- `ConfigurationService.cs` - Settings management
- `CommandPromptForm.cs` - Command generation dialog
- `SettingsForm.cs` - Configuration interface
- `Win32.cs` - Windows API structures

## Security

- **Local Processing**: All AI processing happens locally via Ollama
- **No Network Calls**: No data is sent to external services
- **Command Preview**: Generated commands are always shown before execution
- **Confirmation Required**: Destructive operations require explicit confirmation
- **Configurable Safety**: Adjust safety settings based on your needs

## License

Same license as the main Shorthand project.

## Contributing

Contributions are welcome! Please see the main project's contributing guidelines.

## Support

For issues and questions:
- Check the troubleshooting section above
- Review the configuration options
- Open an issue on GitHub with detailed information about your setup
