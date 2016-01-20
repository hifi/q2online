using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;

namespace Launcher
{
    public class UpdateWindow : Form
    {
        private Label _status;
        private ProgressBar _progressBar;
        private CheckBox _autoLaunch;
        private Button _launchButton;

        public UpdateWindow()
        {
            Text = "Updating " + Configuration.Instance.Name;
            Icon = Configuration.Instance.Icon;

            MaximizeBox = false;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CenterToScreen();

            Padding = Padding.Empty;
            Margin = Padding.Empty;

            var outerPanel = new FlowLayoutPanel();
            var picture = new PictureBox();

            picture.Image = Configuration.Instance.Background;
            picture.ClientSize = new Size(Configuration.Instance.Background.Width, Configuration.Instance.Background.Height);
            picture.Margin = Padding.Empty;
            picture.Margin = new Padding();

            outerPanel.Padding = Padding.Empty;
            outerPanel.Margin = Padding.Empty;
            outerPanel.FlowDirection = FlowDirection.TopDown;
            outerPanel.AutoSize = true;

            outerPanel.Controls.Add(picture);

            var controlPanel = new TableLayoutPanel();
            controlPanel.AutoSize = true;
            controlPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            controlPanel.Margin = new Padding(8);

            _status = new Label();
            _status.Text = "Initializing...";
            _status.AutoSize = true;
            _status.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            controlPanel.Controls.Add(_status);

            _progressBar = new ProgressBar();
            _progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _progressBar.Value = 0;
            controlPanel.Controls.Add(_progressBar);

            _autoLaunch = new CheckBox();
            _autoLaunch.AutoSize = true;
            _autoLaunch.Text = "Launch " + Configuration.Instance.Name + " as soon as it's ready";
            _autoLaunch.Checked = true;
            _autoLaunch.TextAlign = ContentAlignment.BottomLeft;
            controlPanel.Controls.Add(_autoLaunch);

            var buttonPanel = new FlowLayoutPanel();
            buttonPanel.AutoSize = true;
            buttonPanel.Margin = Padding.Empty;

            var newsButton = new Button();
            newsButton.AutoSize = true;
            newsButton.Text = "Update news...";
            newsButton.Click += (object sender, EventArgs e) => {
                System.Diagnostics.Process.Start(Configuration.Instance.NewsURL);
            };
            buttonPanel.Controls.Add(newsButton);

            _launchButton = new Button();
            _launchButton.AutoSize = true;
            _launchButton.Text = "Start Game";
            _launchButton.Enabled = false;
            _launchButton.DialogResult = DialogResult.OK;
            buttonPanel.Controls.Add(_launchButton);

            var cancelButton = new Button();
            cancelButton.AutoSize = true;
            cancelButton.Text = "Cancel";
            cancelButton.DialogResult = DialogResult.Cancel;
            buttonPanel.Controls.Add(cancelButton);

            controlPanel.Controls.Add(buttonPanel);

            outerPanel.Controls.Add(controlPanel);
            Controls.Add(outerPanel);
        }

        public bool Run(List<Archive> archives)
        {
            var updater = new Updater(archives);

            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (object sender, ProgressChangedEventArgs e) => {
                _progressBar.Value = e.ProgressPercentage;
                _status.Text = e.UserState as string;
                this.Refresh();
            };

            updater.StatusChanged += (object sender, UpdaterEventArgs e) => {
                worker.ReportProgress(e.progress, e.status);
            };

            worker.DoWork += (object sender, DoWorkEventArgs e) => {
                updater.Run();
            };

            worker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) => {
                this._launchButton.Enabled = true;

                if (_autoLaunch.Checked) {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            worker.RunWorkerAsync();

            return ShowDialog() == DialogResult.OK;
        }
    }
}

