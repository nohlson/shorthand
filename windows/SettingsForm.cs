using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace Shorthand.Windows
{
    public partial class SettingsForm : Form
    {
        private readonly ConfigurationService configService;
        private ComboBox hotkeyComboBox;
        private TextBox ollamaUrlTextBox;
        private TextBox modelNameTextBox;
        private CheckBox showPreviewCheckBox;
        private CheckBox requireConfirmationCheckBox;
        private ListBox terminalListBox;
        private ComboBox pasteMethodComboBox;
        private TextBox processNamesTextBox;
        private Button saveButton;
        private Button cancelButton;
        private Button addTerminalButton;
        private Button removeTerminalButton;

        public SettingsForm(ConfigurationService configService)
        {
            this.configService = configService;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.hotkeyComboBox = new ComboBox();
            this.ollamaUrlTextBox = new TextBox();
            this.modelNameTextBox = new TextBox();
            this.showPreviewCheckBox = new CheckBox();
            this.requireConfirmationCheckBox = new CheckBox();
            this.terminalListBox = new ListBox();
            this.pasteMethodComboBox = new ComboBox();
            this.processNamesTextBox = new TextBox();
            this.saveButton = new Button();
            this.cancelButton = new Button();
            this.addTerminalButton = new Button();
            this.removeTerminalButton = new Button();
            this.SuspendLayout();

            // Labels
            var hotkeyLabel = new Label { Text = "Hotkey:", Location = new Point(12, 15), AutoSize = true };
            var ollamaLabel = new Label { Text = "Ollama URL:", Location = new Point(12, 45), AutoSize = true };
            var modelLabel = new Label { Text = "Model Name:", Location = new Point(12, 75), AutoSize = true };
            var terminalLabel = new Label { Text = "Terminal Applications:", Location = new Point(12, 105), AutoSize = true };
            var pasteMethodLabel = new Label { Text = "Paste Method:", Location = new Point(250, 135), AutoSize = true };
            var processNamesLabel = new Label { Text = "Process Names:", Location = new Point(250, 165), AutoSize = true };

            // hotkeyComboBox
            this.hotkeyComboBox.Location = new Point(100, 12);
            this.hotkeyComboBox.Size = new Size(150, 23);
            this.hotkeyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.hotkeyComboBox.Items.AddRange(new object[] { "Ctrl+G", "Ctrl+Shift+G", "Alt+G", "F12" });

            // ollamaUrlTextBox
            this.ollamaUrlTextBox.Location = new Point(100, 42);
            this.ollamaUrlTextBox.Size = new Size(200, 23);

            // modelNameTextBox
            this.modelNameTextBox.Location = new Point(100, 72);
            this.modelNameTextBox.Size = new Size(200, 23);

            // showPreviewCheckBox
            this.showPreviewCheckBox.AutoSize = true;
            this.showPreviewCheckBox.Location = new Point(320, 12);
            this.showPreviewCheckBox.Text = "Show command preview";

            // requireConfirmationCheckBox
            this.requireConfirmationCheckBox.AutoSize = true;
            this.requireConfirmationCheckBox.Location = new Point(320, 35);
            this.requireConfirmationCheckBox.Text = "Require confirmation";

            // terminalListBox
            this.terminalListBox.Location = new Point(12, 125);
            this.terminalListBox.Size = new Size(200, 120);
            this.terminalListBox.SelectedIndexChanged += TerminalListBox_SelectedIndexChanged;

            // pasteMethodComboBox
            this.pasteMethodComboBox.Location = new Point(350, 132);
            this.pasteMethodComboBox.Size = new Size(150, 23);
            this.pasteMethodComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.pasteMethodComboBox.Items.AddRange(new object[] { "Ctrl+V", "Shift+Insert", "Right Click", "Custom" });

            // processNamesTextBox
            this.processNamesTextBox.Location = new Point(350, 162);
            this.processNamesTextBox.Size = new Size(150, 23);
            this.processNamesTextBox.PlaceholderText = "process1,process2";

            // addTerminalButton
            this.addTerminalButton.Location = new Point(12, 255);
            this.addTerminalButton.Size = new Size(95, 23);
            this.addTerminalButton.Text = "Add Terminal";
            this.addTerminalButton.Click += AddTerminalButton_Click;

            // removeTerminalButton
            this.removeTerminalButton.Location = new Point(117, 255);
            this.removeTerminalButton.Size = new Size(95, 23);
            this.removeTerminalButton.Text = "Remove Terminal";
            this.removeTerminalButton.Click += RemoveTerminalButton_Click;

            // saveButton
            this.saveButton.Location = new Point(350, 255);
            this.saveButton.Size = new Size(75, 23);
            this.saveButton.Text = "Save";
            this.saveButton.Click += SaveButton_Click;

            // cancelButton
            this.cancelButton.Location = new Point(431, 255);
            this.cancelButton.Size = new Size(75, 23);
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += CancelButton_Click;

            // SettingsForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(518, 290);
            this.Controls.AddRange(new Control[] {
                hotkeyLabel, this.hotkeyComboBox,
                ollamaLabel, this.ollamaUrlTextBox,
                modelLabel, this.modelNameTextBox,
                this.showPreviewCheckBox, this.requireConfirmationCheckBox,
                terminalLabel, this.terminalListBox,
                pasteMethodLabel, this.pasteMethodComboBox,
                processNamesLabel, this.processNamesTextBox,
                this.addTerminalButton, this.removeTerminalButton,
                this.saveButton, this.cancelButton
            });
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Shorthand Settings";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSettings()
        {
            var config = configService.Configuration;
            
            hotkeyComboBox.Text = config.Hotkey;
            ollamaUrlTextBox.Text = config.OllamaUrl;
            modelNameTextBox.Text = config.ModelName;
            showPreviewCheckBox.Checked = config.ShowCommandPreview;
            requireConfirmationCheckBox.Checked = config.RequireConfirmation;

            // Load terminal applications
            terminalListBox.Items.Clear();
            foreach (var kvp in config.TerminalApplications)
            {
                terminalListBox.Items.Add(kvp.Key);
            }

            if (terminalListBox.Items.Count > 0)
            {
                terminalListBox.SelectedIndex = 0;
            }
        }

        private void TerminalListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (terminalListBox.SelectedItem is string terminalName)
            {
                var config = configService.Configuration.TerminalApplications[terminalName];
                pasteMethodComboBox.Text = config.PasteMethod.ToString();
                processNamesTextBox.Text = string.Join(",", config.ProcessNames);
            }
        }

        private void AddTerminalButton_Click(object? sender, EventArgs e)
        {
            var terminalName = Interaction.InputBox(
                "Enter terminal application name:", "Add Terminal", "");
            
            if (!string.IsNullOrWhiteSpace(terminalName))
            {
                var newConfig = new TerminalConfig
                {
                    PasteMethod = PasteMethod.CtrlV,
                    ProcessNames = new[] { terminalName.ToLower() }
                };
                
                configService.UpdateTerminalConfig(terminalName, newConfig);
                terminalListBox.Items.Add(terminalName);
                terminalListBox.SelectedItem = terminalName;
            }
        }

        private void RemoveTerminalButton_Click(object? sender, EventArgs e)
        {
            if (terminalListBox.SelectedItem is string terminalName)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove '{terminalName}'?",
                    "Confirm Removal",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    configService.Configuration.TerminalApplications.Remove(terminalName);
                    terminalListBox.Items.Remove(terminalName);
                    
                    if (terminalListBox.Items.Count > 0)
                    {
                        terminalListBox.SelectedIndex = 0;
                    }
                }
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Update current terminal config if one is selected
                if (terminalListBox.SelectedItem is string terminalName)
                {
                    var config = configService.Configuration.TerminalApplications[terminalName];
                    
                    if (Enum.TryParse<PasteMethod>(pasteMethodComboBox.Text, out var pasteMethod))
                    {
                        config.PasteMethod = pasteMethod;
                    }
                    
                    var processNames = processNamesTextBox.Text.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();
                    
                    if (processNames.Length > 0)
                    {
                        config.ProcessNames = processNames;
                    }
                }

                // Update general settings
                configService.Configuration.Hotkey = hotkeyComboBox.Text;
                configService.Configuration.OllamaUrl = ollamaUrlTextBox.Text;
                configService.Configuration.ModelName = modelNameTextBox.Text;
                configService.Configuration.ShowCommandPreview = showPreviewCheckBox.Checked;
                configService.Configuration.RequireConfirmation = requireConfirmationCheckBox.Checked;

                configService.SaveConfiguration();
                
                MessageBox.Show("Settings saved successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}
