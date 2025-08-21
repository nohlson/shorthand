using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shorthand.Windows
{
    public partial class CommandPromptForm : Form
    {
        private readonly OllamaService ollamaService;
        private readonly ConfigurationService configService;
        private TextBox promptTextBox;
        private Button generateButton;
        private Button cancelButton;
        private ListBox commandListBox;
        private CheckBox unsafeCheckBox;
        private Label statusLabel;
        private Button useCommandButton;

        public event EventHandler<string>? CommandGenerated;

        public CommandPromptForm(OllamaService ollamaService, ConfigurationService configService)
        {
            this.ollamaService = ollamaService;
            this.configService = configService;
            InitializeComponent();
            SetupForm();
        }

        private void InitializeComponent()
        {
            this.promptTextBox = new TextBox();
            this.generateButton = new Button();
            this.cancelButton = new Button();
            this.commandListBox = new ListBox();
            this.unsafeCheckBox = new CheckBox();
            this.statusLabel = new Label();
            this.useCommandButton = new Button();
            this.SuspendLayout();

            // promptTextBox
            this.promptTextBox.Location = new Point(12, 12);
            this.promptTextBox.Name = "promptTextBox";
            this.promptTextBox.Size = new Size(400, 23);
            this.promptTextBox.TabIndex = 0;
            this.promptTextBox.PlaceholderText = "Describe the command you want to generate...";
            this.promptTextBox.KeyDown += PromptTextBox_KeyDown;

            // generateButton
            this.generateButton.Location = new Point(418, 12);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new Size(75, 23);
            this.generateButton.TabIndex = 1;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += GenerateButton_Click;

            // unsafeCheckBox
            this.unsafeCheckBox.AutoSize = true;
            this.unsafeCheckBox.Location = new Point(12, 45);
            this.unsafeCheckBox.Name = "unsafeCheckBox";
            this.unsafeCheckBox.Size = new Size(100, 19);
            this.unsafeCheckBox.TabIndex = 2;
            this.unsafeCheckBox.Text = "Allow unsafe commands";

            // statusLabel
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new Point(120, 45);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new Size(0, 15);
            this.statusLabel.TabIndex = 3;

            // commandListBox
            this.commandListBox.Location = new Point(12, 70);
            this.commandListBox.Name = "commandListBox";
            this.commandListBox.Size = new Size(481, 100);
            this.commandListBox.TabIndex = 4;
            this.commandListBox.SelectedIndexChanged += CommandListBox_SelectedIndexChanged;

            // useCommandButton
            this.useCommandButton.Enabled = false;
            this.useCommandButton.Location = new Point(12, 176);
            this.useCommandButton.Name = "useCommandButton";
            this.useCommandButton.Size = new Size(100, 23);
            this.useCommandButton.TabIndex = 5;
            this.useCommandButton.Text = "Use Command";
            this.useCommandButton.UseVisualStyleBackColor = true;
            this.useCommandButton.Click += UseCommandButton_Click;

            // cancelButton
            this.cancelButton.Location = new Point(418, 176);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += CancelButton_Click;

            // CommandPromptForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(505, 211);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.useCommandButton);
            this.Controls.Add(this.commandListBox);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.unsafeCheckBox);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.promptTextBox);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CommandPromptForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Generate Command";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void SetupForm()
        {
            // Focus the prompt textbox
            promptTextBox.Focus();
            
            // Set initial status
            UpdateStatus("Ready to generate commands");
        }

        private void PromptTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                GenerateCommand();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private async void GenerateButton_Click(object? sender, EventArgs e)
        {
            await GenerateCommand();
        }

        private async Task GenerateCommand()
        {
            var prompt = promptTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                UpdateStatus("Please enter a prompt");
                return;
            }

            try
            {
                UpdateStatus("Generating command...");
                generateButton.Enabled = false;

                var command = await ollamaService.GenerateCommandAsync(prompt, unsafeCheckBox.Checked);
                
                if (!string.IsNullOrWhiteSpace(command))
                {
                    commandListBox.Items.Clear();
                    commandListBox.Items.Add(command);
                    commandListBox.SelectedIndex = 0;
                    UpdateStatus("Command generated successfully");
                }
                else
                {
                    UpdateStatus("No command generated");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
                MessageBox.Show($"Failed to generate command: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                generateButton.Enabled = true;
            }
        }

        private void CommandListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            useCommandButton.Enabled = commandListBox.SelectedIndex >= 0;
        }

        private void UseCommandButton_Click(object? sender, EventArgs e)
        {
            if (commandListBox.SelectedItem is string selectedCommand)
            {
                if (configService.Configuration.RequireConfirmation)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to execute this command?\n\n{selectedCommand}",
                        "Confirm Command",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        CommandGenerated?.Invoke(this, selectedCommand);
                        Close();
                    }
                }
                else
                {
                    CommandGenerated?.Invoke(this, selectedCommand);
                    Close();
                }
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = Color.Black;
        }

        private void UpdateStatus(string message, Color color)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = color;
        }
    }
}
