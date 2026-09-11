using System;
using System.Windows.Forms;

namespace Plugin_ListenerCount
{
    internal class ConfigForm : Form
    {
        private readonly TextBox urlTextBox;
        private readonly NumericUpDown intervalUpDown;
        private readonly TextBox outputPathTextBox;

        public string Url => urlTextBox.Text.Trim();
        public int IntervalMinutes => (int)intervalUpDown.Value;
        public string OutputPath => outputPathTextBox.Text.Trim();

        public ConfigForm(string url, int intervalMinutes, string outputPath)
        {
            Text = "Listener Count Importer - Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new System.Drawing.Size(460, 190);

            var urlLabel = new Label { Text = "URL:", Left = 12, Top = 15, Width = 100 };
            urlTextBox = new TextBox { Text = url, Left = 120, Top = 12, Width = 320 };

            var intervalLabel = new Label { Text = "Poll interval (minutes):", Left = 12, Top = 45, Width = 100 };
            intervalUpDown = new NumericUpDown
            {
                Left = 120,
                Top = 42,
                Width = 80,
                Minimum = 1,
                Maximum = 1440,
                Value = Math.Min(Math.Max(intervalMinutes, 1), 1440),
            };

            var outputLabel = new Label { Text = "Output file:", Left = 12, Top = 75, Width = 100 };
            outputPathTextBox = new TextBox { Text = outputPath, Left = 120, Top = 72, Width = 250 };
            var browseButton = new Button { Text = "Browse...", Left = 378, Top = 71, Width = 62 };
            browseButton.Click += (s, e) => BrowseForOutputPath();

            var okButton = new Button { Text = "Save", Left = 288, Top = 145, Width = 75, DialogResult = DialogResult.OK };
            var cancelButton = new Button { Text = "Cancel", Left = 372, Top = 145, Width = 75, DialogResult = DialogResult.Cancel };
            okButton.Click += (s, e) => ValidateAndAccept();

            AcceptButton = okButton;
            CancelButton = cancelButton;

            Controls.AddRange(new Control[]
            {
                urlLabel, urlTextBox,
                intervalLabel, intervalUpDown,
                outputLabel, outputPathTextBox, browseButton,
                okButton, cancelButton,
            });
        }

        private void BrowseForOutputPath()
        {
            using (var dialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = System.IO.Path.GetFileName(outputPathTextBox.Text),
                InitialDirectory = GetExistingDirectory(outputPathTextBox.Text),
                OverwritePrompt = false,
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    outputPathTextBox.Text = dialog.FileName;
                }
            }
        }

        private static string GetExistingDirectory(string path)
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(path);
                return !string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir) ? dir : @"C:\RadioDJv3";
            }
            catch
            {
                return @"C:\RadioDJv3";
            }
        }

        private void ValidateAndAccept()
        {
            if (!Uri.TryCreate(urlTextBox.Text.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show(this, "Enter a valid http:// or https:// URL.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            if (string.IsNullOrWhiteSpace(outputPathTextBox.Text))
            {
                MessageBox.Show(this, "Enter an output file path.", "Invalid path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
