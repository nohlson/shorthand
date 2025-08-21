using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Shorthand.Windows
{
    public class ConfigurationService
    {
        private readonly string configPath;
        private Configuration config;

        public ConfigurationService()
        {
            configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Shorthand",
                "config.json"
            );
            config = LoadConfiguration();
        }

        public Configuration Configuration => config;

        public void SaveConfiguration()
        {
            try
            {
                var directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                // Log error or show user notification
                System.Diagnostics.Debug.WriteLine($"Failed to save configuration: {ex.Message}");
            }
        }

        private Configuration LoadConfiguration()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var loaded = JsonSerializer.Deserialize<Configuration>(json);
                    if (loaded != null)
                    {
                        return loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load configuration: {ex.Message}");
            }

            // Return default configuration
            return new Configuration
            {
                TerminalApplications = new Dictionary<string, TerminalConfig>
                {
                    ["mobaxterm"] = new TerminalConfig { PasteMethod = PasteMethod.ShiftInsert, ProcessNames = new[] { "mobaxterm" } },
                    ["putty"] = new TerminalConfig { PasteMethod = PasteMethod.RightClick, ProcessNames = new[] { "putty" } },
                    ["windows_terminal"] = new TerminalConfig { PasteMethod = PasteMethod.CtrlV, ProcessNames = new[] { "WindowsTerminal", "wt" } },
                    ["powershell"] = new TerminalConfig { PasteMethod = PasteMethod.CtrlV, ProcessNames = new[] { "powershell", "pwsh" } },
                    ["cmd"] = new TerminalConfig { PasteMethod = PasteMethod.CtrlV, ProcessNames = new[] { "cmd", "conhost" } }
                },
                OllamaUrl = "http://127.0.0.1:11434",
                ModelName = "cmdgen",
                Hotkey = "Ctrl+G",
                ShowCommandPreview = true,
                RequireConfirmation = false
            };
        }

        public void UpdateTerminalConfig(string terminalName, TerminalConfig config)
        {
            if (this.config.TerminalApplications.ContainsKey(terminalName))
            {
                this.config.TerminalApplications[terminalName] = config;
            }
            else
            {
                this.config.TerminalApplications.Add(terminalName, config);
            }
            SaveConfiguration();
        }

        public TerminalConfig? GetTerminalConfig(string processName)
        {
            foreach (var kvp in config.TerminalApplications)
            {
                if (kvp.Value.ProcessNames.Contains(processName, StringComparer.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
            return null;
        }
    }

    public class Configuration
    {
        public Dictionary<string, TerminalConfig> TerminalApplications { get; set; } = new();
        public string OllamaUrl { get; set; } = "http://127.0.0.1:11434";
        public string ModelName { get; set; } = "cmdgen";
        public string Hotkey { get; set; } = "Ctrl+G";
        public bool ShowCommandPreview { get; set; } = true;
        public bool RequireConfirmation { get; set; } = false;
    }

    public class TerminalConfig
    {
        public PasteMethod PasteMethod { get; set; } = PasteMethod.CtrlV;
        public string[] ProcessNames { get; set; } = Array.Empty<string>();
        public string? CustomPasteCommand { get; set; }
    }

    public enum PasteMethod
    {
        CtrlV,
        ShiftInsert,
        RightClick,
        Custom
    }
}
