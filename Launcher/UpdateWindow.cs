using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

namespace Launcher
{
    public class UpdateWindow : Form
    {
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

            var status = new Label();
            status.Text = "Updating...";
            status.AutoSize = true;
            status.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            controlPanel.Controls.Add(status);

            var progress = new ProgressBar();
            progress.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            progress.Value = 0;
            controlPanel.Controls.Add(progress);

            var autoLaunch = new CheckBox();
            autoLaunch.AutoSize = true;
            autoLaunch.Text = "Launch " + Configuration.Instance.Name + " as soon as it's ready";
            autoLaunch.Checked = true;
            autoLaunch.TextAlign = ContentAlignment.BottomLeft;
            controlPanel.Controls.Add(autoLaunch);

            var buttonPanel = new FlowLayoutPanel();
            buttonPanel.AutoSize = true;
            buttonPanel.Margin = Padding.Empty;

            var newsButton = new Button();
            newsButton.AutoSize = true;
            newsButton.Text = "Update news...";
            buttonPanel.Controls.Add(newsButton);

            var launchButton = new Button();
            launchButton.AutoSize = true;
            launchButton.Text = "Launch";
            launchButton.Enabled = false;
            buttonPanel.Controls.Add(launchButton);

            var cancelButton = new Button();
            cancelButton.AutoSize = true;
            cancelButton.Text = "Cancel";
            buttonPanel.Controls.Add(cancelButton);

            controlPanel.Controls.Add(buttonPanel);

            outerPanel.Controls.Add(controlPanel);
            Controls.Add(outerPanel);
        }

        public void Run(List<Archive> archives)
        {
            ShowDialog();
        }
    }
}

