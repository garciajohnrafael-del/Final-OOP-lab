using System;
using System.Drawing;
using System.Windows.Forms;
using OOP10FEXLab.Kiosk;
using OOP10FEXLab.QueueMonitor;

namespace OOP10FEXLab
{
    /// <summary>Start screen: lets the tester open the Kiosk, the Queue Monitor, or both side by side.</summary>
    public class LauncherForm : Form
    {
        public LauncherForm()
        {
            Text = "OOP10 FEX Lab - Launcher";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ClientSize = new Size(420, 300);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10f);

            var title = new Label
            {
                Text = "QuickBite Ordering System",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 16, 46),
                AutoSize = false, TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top, Height = 70
            };

            Button btnKiosk = MakeButton("Open Kiosk", 90);
            Button btnMonitor = MakeButton("Open Queue Monitor", 145);
            Button btnBoth = MakeButton("Open Both (demo)", 200);

            btnKiosk.Click += (s, e) => new KioskForm().Show();
            btnMonitor.Click += (s, e) => new QueueMonitorForm().Show();
            btnBoth.Click += (s, e) => OpenBoth();

            Controls.Add(btnKiosk);
            Controls.Add(btnMonitor);
            Controls.Add(btnBoth);
            Controls.Add(title);
        }

        private Button MakeButton(string text, int top)
        {
            return new Button
            {
                Text = text,
                Left = 80, Top = top, Width = 260, Height = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 16, 46),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private void OpenBoth()
        {
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            int half = area.Width / 2;

            var kiosk = new KioskForm { StartPosition = FormStartPosition.Manual };
            kiosk.Bounds = new Rectangle(area.Left, area.Top, half, area.Height);

            var monitor = new QueueMonitorForm { StartPosition = FormStartPosition.Manual };
            monitor.Bounds = new Rectangle(area.Left + half, area.Top, half, area.Height);

            kiosk.Show();
            monitor.Show();
        }
    }
}
