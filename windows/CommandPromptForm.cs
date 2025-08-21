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
        private TextBox commandTextBox;
        private CheckBox unsafeCheckBox;
        private Label statusLabel;
        private Button useCommandButton;
        private ToolTip toolTip;

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
            this.commandTextBox = new TextBox();
            this.unsafeCheckBox = new CheckBox();
            this.statusLabel = new Label();
            this.useCommandButton = new Button();
            this.toolTip = new ToolTip();
            this.SuspendLayout();

            // promptTextBox
            this.promptTextBox.Location = new Point(12, 12);
            this.promptTextBox.Name = "promptTextBox";
            this.promptTextBox.Size = new Size(400, 23);
            this.promptTextBox.TabIndex = 0;
            this.promptTextBox.PlaceholderText = "Describe the command you want to generate...";
            this.promptTextBox.KeyDown += PromptTextBox_KeyDown;
            this.toolTip.SetToolTip(this.promptTextBox, "Type your prompt here and press Enter to generate a command");

            // generateButton
            this.generateButton.Location = new Point(418, 12);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new Size(75, 23);
            this.generateButton.TabIndex = 1;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += GenerateButton_Click;
            this.toolTip.SetToolTip(this.generateButton, "Generate a command from your prompt");

            // unsafeCheckBox
            this.unsafeCheckBox.AutoSize = true;
            this.unsafeCheckBox.Location = new Point(12, 45);
            this.unsafeCheckBox.Name = "unsafeCheckBox";
            this.unsafeCheckBox.Size = new Size(150, 19);
            this.unsafeCheckBox.TabIndex = 2;
            this.unsafeCheckBox.Text = "Allow unsafe commands";
            this.toolTip.SetToolTip(this.unsafeCheckBox, "Check this if you need to generate potentially destructive commands");

            // statusLabel
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new Point(170, 45);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new Size(0, 15);
            this.statusLabel.TabIndex = 3;

            // commandTextBox
            this.commandTextBox.Location = new Point(12, 70);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new Size(481, 23);
            this.commandTextBox.TabIndex = 4;
            this.commandTextBox.ReadOnly = true;
            this.commandTextBox.BackColor = SystemColors.Control;
            this.commandTextBox.ForeColor = SystemColors.ControlText;
            this.commandTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular);
            this.commandTextBox.KeyDown += CommandTextBox_KeyDown;
            this.toolTip.SetToolTip(this.commandTextBox, "Generated command - Press Enter to use it");

            // useCommandButton
            this.useCommandButton.Enabled = false;
            this.useCommandButton.Location = new Point(12, 103);
            this.useCommandButton.Name = "useCommandButton";
            this.useCommandButton.Size = new Size(120, 23);
            this.useCommandButton.TabIndex = 5;
            this.useCommandButton.Text = "Use Command (Enter)";
            this.useCommandButton.UseVisualStyleBackColor = true;
            this.useCommandButton.Click += UseCommandButton_Click;
            this.toolTip.SetToolTip(this.useCommandButton, "Use the generated command (or press Enter in the command textbox)");

            // cancelButton
            this.cancelButton.Location = new Point(418, 103);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += CancelButton_Click;
            this.toolTip.SetToolTip(this.cancelButton, "Cancel and close the form");

            // CommandPromptForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(525, 138);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.useCommandButton);
            this.Controls.Add(this.commandTextBox);
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
            // Set initial status with keyboard shortcut hint
            UpdateStatus("Type your prompt and press Enter to generate, or Enter again to use the command");
            
            // Subscribe to the Shown event to ensure proper focus
            this.Shown += CommandPromptForm_Shown;
        }

        private void PromptTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (!string.IsNullOrWhiteSpace(commandTextBox.Text))
                {
                    // If a command is already generated, use it immediately
                    UpdateStatus("Using generated command...");
                    UseGeneratedCommand();
                    return;
                }
                else
                {
                    // Otherwise generate a new command
                    GenerateCommand();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                UpdateStatus("Cancelling via Escape...");
                Close();
            }
            else if (e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true;
                // Tab to the unsafe checkbox
                UpdateStatus("Tabbed to unsafe checkbox");
                unsafeCheckBox.Focus();
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                // F5 to regenerate command
                UpdateStatus("Regenerating command with F5...");
                GenerateCommand();
            }
        }

        private async void GenerateButton_Click(object? sender, EventArgs e)
        {
            UpdateStatus("Generating command via button...");
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
                    commandTextBox.Text = command;
                    UpdateStatus("Command generated successfully - Press Enter to use it");
                    
                    // Auto-focus the command textbox to show it's ready
                    commandTextBox.Focus();
                    commandTextBox.SelectAll();
                    
                    // Enable the use command button
                    useCommandButton.Enabled = true;
                }
                else
                {
                    UpdateStatus("No command generated - try a different prompt");
                    commandTextBox.Text = "";
                    useCommandButton.Enabled = false;
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

        private void UseGeneratedCommand()
        {
            var command = commandTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(command))
            {
                Console.WriteLine($"RequireConfirmation: {configService.Configuration.RequireConfirmation}");
                if (configService.Configuration.RequireConfirmation)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to execute this command?\n\n{command}",
                        "Confirm Command",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        CommandGenerated?.Invoke(this, command);
                        Close();
                    }
                }
                else
                {
                    CommandGenerated?.Invoke(this, command);
                    Close();
                }
            }
        }

        private void UseCommandButton_Click(object? sender, EventArgs e)
        {
            UpdateStatus("Using command via button...");
            UseGeneratedCommand();
        }

        private void CommandTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                UpdateStatus("Using generated command...");
                UseGeneratedCommand();
            }
            else if (e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true;
                // Tab to the use command button
                UpdateStatus("Tabbed to use command button");
                useCommandButton.Focus();
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            UpdateStatus("Cancelling via button...");
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

        private void CommandPromptForm_Shown(object? sender, EventArgs e)
        {
            // Use BeginInvoke to ensure this runs after the form is fully shown
            this.BeginInvoke(new Action(() =>
            {
                // Ensure the form gets focus and the prompt textbox is ready for input
                this.Focus();
                this.Activate();
                this.BringToFront();
                
                // Focus the prompt textbox and select all text for immediate typing
                promptTextBox.Focus();
                promptTextBox.SelectAll();
                
                // Force the form to be the active window
                this.TopMost = true;
                
                // Additional focus enforcement with a small delay
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 100;
                timer.Tick += (s, args) =>
                {
                    this.Focus();
                    promptTextBox.Focus();
                    promptTextBox.SelectAll();
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }));
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            // Additional focus handling when the form is shown
            this.BeginInvoke(new Action(() =>
            {
                this.Focus();
                promptTextBox.Focus();
                promptTextBox.SelectAll();
            }));
        }
    }
}
