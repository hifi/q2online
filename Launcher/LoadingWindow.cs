using System;
using System.Drawing;
using System.Windows.Forms;

namespace Launcher
{
    public class LoadingWindow : Form
    {
        protected Font _font;
        protected SolidBrush _white;
        protected SolidBrush _black;
        protected StringFormat _format;
        protected String _status;

        public string Status
        {
            get { return _status; }
            set {
                _status = value;
                Console.WriteLine(_status);

                Invoke((MethodInvoker)delegate() {
                    Show();
                    Refresh();
                });
            }
        }

        public static LoadingWindow Create()
        {
            return new LoadingWindow(Configuration.Instance.Name, Configuration.Instance.Background, Configuration.Instance.Icon);
        }

        public LoadingWindow (string title, Image image, Icon icon)
        {
            Text = title;
            Size = new Size(image.Width, image.Height);
            BackgroundImage = image;
            Icon = icon;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = true;
            DoubleBuffered = true;
            _font = new Font("SansSerif", 10);
            _white = new SolidBrush(Color.White);
            _black = new SolidBrush(Color.Black);
            _format = new StringFormat();
            _format.Alignment = StringAlignment.Far;
            _format.LineAlignment = StringAlignment.Far;
            CenterToScreen();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawString(_status, _font, _black, new RectangleF(-3, -3, Width, Height), _format);
            e.Graphics.DrawString(_status, _font, _white, new RectangleF(-4, -4, Width, Height), _format);
        }
    }
}

