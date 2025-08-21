using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks; // Added for Task.Delay

namespace Shorthand.Windows
{
    public partial class MainForm : Form
    {
        private NotifyIcon trayIcon;
        private GlobalHotkey hotkey;
        private OllamaService ollamaService;
        private ConfigurationService configService;
        private HotkeyMessageFilter messageFilter;
        private IntPtr lastTerminalWindow = IntPtr.Zero; // Added for storing terminal window handle
        private CommandPromptForm? currentPromptForm; // Track the current shorthand window

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            InitializeTrayIcon();
            InitializeHotkey();
        }

        private void InitializeServices()
        {
            configService = new ConfigurationService();
            ollamaService = new OllamaService();
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Shorthand - Press Ctrl+G to generate commands",
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Settings", null, (s, e) => ShowSettings());
            contextMenu.Items.Add("About", null, (s, e) => ShowAbout());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
            contextMenu.Items.Add("Test Hotkey", null, (s, e) => ShowCommandPrompt());

            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.DoubleClick += (s, e) => ShowSettings();
        }

        private void InitializeHotkey()
        {
            hotkey = new GlobalHotkey();
            if (hotkey.RegisterHotkey(Keys.G, (ModifierKeys)0x0002))
            {
                // Hotkey registered successfully
                messageFilter = new HotkeyMessageFilter(hotkey);
                Application.AddMessageFilter(messageFilter);
                trayIcon.ShowBalloonTip(3000, "Shorthand", "Hotkey Ctrl+G registered successfully!", ToolTipIcon.Info);
            }
            else
            {
                // Hotkey registration failed
                trayIcon.ShowBalloonTip(3000, "Shorthand Error", "Failed to register Ctrl+G hotkey!", ToolTipIcon.Error);
            }
            hotkey.HotkeyPressed += OnHotkeyPressed;
        }

        private void OnHotkeyPressed(object? sender, EventArgs e)
        {
            // Store the terminal window that triggered the hotkey
            lastTerminalWindow = GetForegroundWindow();

            Console.WriteLine($"Last terminal window: {lastTerminalWindow}");
            
            if (IsTerminalWindowActive())
            {
                ShowCommandPrompt();
            }
            else
            {
                trayIcon.ShowBalloonTip(3000, "Shorthand", "No terminal window detected. Please focus a terminal application first.", ToolTipIcon.Info);
            }
        }

        private bool IsTerminalWindowActive()
        {
            var activeWindow = GetForegroundWindow();
            if (activeWindow == IntPtr.Zero) return false;

            var windowTitle = GetWindowTitle(activeWindow);
            var processName = GetProcessName(activeWindow);

            var terminalApps = new[] { "WindowsTerminal" };
            var terminalTitles = new[] { "" };

            // Print processName
            trayIcon.ShowBalloonTip(3000, "Shorthand", $"ProcessName: {processName}", ToolTipIcon.Info);
            return terminalApps.Any(app => processName.Contains(app, StringComparison.OrdinalIgnoreCase)) ||
                   terminalTitles.Any(title => windowTitle.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        private void ShowCommandPrompt()
        {
            // If the form is already open, close it
            if (IsShorthandWindowOpen())
            {
                CloseShorthandWindow();
                return;
            }

            // Create and show the new form
            OpenShorthandWindow();
        }

        private bool IsShorthandWindowOpen()
        {
            return currentPromptForm != null && !currentPromptForm.IsDisposed && currentPromptForm.Visible;
        }

        private void OpenShorthandWindow()
        {
            currentPromptForm = new CommandPromptForm(ollamaService, configService);
            currentPromptForm.CommandGenerated += OnCommandGenerated;
            currentPromptForm.FormClosed += (s, e) => 
            {
                // Clear reference when closed by any means
                currentPromptForm = null;
            };
            
            // Show as dialog - this will automatically handle focus and modal behavior
            currentPromptForm.ShowDialog();
        }

        private void CloseShorthandWindow()
        {
            if (currentPromptForm != null && !currentPromptForm.IsDisposed)
            {
                currentPromptForm.Close();
                // Don't set to null here - let the FormClosed event handle it
            }
        }

        private void OnCommandGenerated(object? sender, string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;
            SendCommandToTerminal(command);
            
            // The FormClosed event will handle clearing the reference
        }

        private void SendCommandToTerminal(string command)
        {
            try
            {
                // Use the stored terminal window instead of getting current focus
                if (lastTerminalWindow != IntPtr.Zero)
                {
                    // Set focus back to the terminal window
                    SetForegroundWindow(lastTerminalWindow);

                    Console.WriteLine($"Set focus to: {lastTerminalWindow}");
                    
                    // Wait a moment for focus to change
                    System.Threading.Thread.Sleep(100);
                    
                    // Now send the command
                    Clipboard.SetText(command);
                    System.Threading.Thread.Sleep(50);
                    SendKeys.SendWait("^v");
                    
                    trayIcon.ShowBalloonTip(2000, "Shorthand", "Command sent to terminal", ToolTipIcon.Info);
                }
                else
                {
                    // Fallback: copy to clipboard
                    Clipboard.SetText(command);
                    trayIcon.ShowBalloonTip(3000, "Shorthand", "Command copied to clipboard. Please paste manually.", ToolTipIcon.Info);
                }
            }
            catch (Exception ex)
            {
                trayIcon.ShowBalloonTip(3000, "Shorthand Error", $"Failed to send command: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private void ShowSettings()
        {
            var settingsForm = new SettingsForm(configService);
            settingsForm.ShowDialog();
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "Shorthand for Windows\n\n" +
                "Local-only prompt-to-command generator\n" +
                "Press Ctrl+G in any terminal to generate commands\n\n" +
                "Version 1.0.0",
                "About Shorthand",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                trayIcon.Visible = false;
                hotkey?.Dispose();
            }
            base.OnFormClosing(e);
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false); // Always hide the form
        }

        private class HotkeyMessageFilter : IMessageFilter
        {
            private readonly GlobalHotkey hotkey;
            private const int WM_HOTKEY = 0x0312;

            public HotkeyMessageFilter(GlobalHotkey hotkey)
            {
                this.hotkey = hotkey;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    bool handled = false;
                    hotkey.WndProc(IntPtr.Zero, m.Msg, m.WParam, m.LParam, ref handled);
                    return handled;
                }
                return false;
            }
        }

        #region Win32 API

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetModuleFileName(IntPtr hModule, System.Text.StringBuilder lpFilename, int nSize);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd); // Added for SetForegroundWindow

        private string GetWindowTitle(IntPtr hWnd)
        {
            var title = new System.Text.StringBuilder(256);
            GetWindowText(hWnd, title, title.Capacity);
            return title.ToString();
        }

        private string GetProcessName(IntPtr hWnd)
        {
            try
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                
                // Use .NET Process class instead of Win32 API
                var process = System.Diagnostics.Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
            catch (Exception ex)
            {
                // Debug: Show any errors
                trayIcon.ShowBalloonTip(3000, "Debug", $"GetProcessName error: {ex.Message}", ToolTipIcon.Info);
                return string.Empty;
            }
        }

        #endregion

    }
}
