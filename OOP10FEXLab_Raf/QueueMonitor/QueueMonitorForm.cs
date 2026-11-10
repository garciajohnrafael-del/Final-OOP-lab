using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using OOP10FEXLab.Core;

namespace OOP10FEXLab.QueueMonitor
{
    /// <summary>
    /// CLAIM / QUEUE MONITOR (Raf's part)
    /// Left column  = PREPARING   (small numbers)
    /// Right column = NOW SERVING (big red numbers)
    /// Reads transactions.txt every 2 seconds, so new kiosk orders appear automatically.
    /// The bottom crew bar moves orders along; press F2 to hide/show it (customer view).
    /// </summary>
    public class QueueMonitorForm : Form
    {
        private static readonly Color Sky = Color.FromArgb(120, 180, 235);
        private static readonly Color Navy = Color.FromArgb(20, 40, 80);
        private static readonly Color Red = Color.FromArgb(200, 16, 46);

        private readonly QueueService _queue;
        private readonly Timer _refreshTimer = new Timer { Interval = 2000 };
        private readonly Timer _blinkTimer = new Timer { Interval = 400 };
        private string _lastSignature = "";
        private int _blinkTicks;

        private FlowLayoutPanel _preparingPanel, _servingPanel;
        private Label _clockLabel, _countLabel;
        private Panel _crewBar;
        private ComboBox _preparingCombo, _servingCombo;

        public QueueMonitorForm()
        {
            _queue = new QueueService(AppServices.Transactions);

            Text = "QuickBite - Claim Monitor";
            Font = new Font("Segoe UI", 10f);
            MinimumSize = new Size(800, 550);
            Size = new Size(1100, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Sky;
            KeyPreview = true;

            BuildLayout();

            _refreshTimer.Tick += (s, e) => RefreshQueue();
            _blinkTimer.Tick += (s, e) => Blink();
            KeyDown += OnKeyDown;

            RefreshQueue();
            _refreshTimer.Start();
        }

        // ================= LAYOUT =================

        private void BuildLayout()
        {
            // two columns like the reference photo
            var columns = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(15, 5, 15, 5) };
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            columns.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            columns.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            columns.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            columns.Controls.Add(ColumnHeader("PREPARING", Navy), 0, 0);
            columns.Controls.Add(ColumnHeader("NOW SERVING", Red), 1, 0);

            _preparingPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
            _servingPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
            columns.Controls.Add(_preparingPanel, 0, 1);
            columns.Controls.Add(_servingPanel, 1, 1);

            // header bar
            var header = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Navy };
            var title = new Label
            {
                Text = "CLAIM MONITOR", ForeColor = Color.White,
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                Dock = DockStyle.Left, Width = 400, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            _clockLabel = new Label
            {
                ForeColor = Color.White, Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Dock = DockStyle.Right, Width = 260, TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 15, 0)
            };
            header.Controls.Add(title);
            header.Controls.Add(_clockLabel);

            _crewBar = BuildCrewBar();

            Controls.Add(columns);
            Controls.Add(_crewBar);
            Controls.Add(header);
        }

        private Label ColumnHeader(string text, Color color)
        {
            return new Label
            {
                Text = text, Dock = DockStyle.Fill, ForeColor = color,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private Panel BuildCrewBar()
        {
            var bar = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Navy, Padding = new Padding(10) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };

            var lbl1 = CrewLabel("Preparing:");
            _preparingCombo = CrewCombo();
            Button ready = CrewButton("Order Ready >", Color.FromArgb(255, 205, 0), Color.Black);
            ready.Click += (s, e) => MoveSelected(_preparingCombo, true);

            var lbl2 = CrewLabel("Now Serving:");
            _servingCombo = CrewCombo();
            Button claimed = CrewButton("Claimed", Color.White, Red);
            claimed.Click += (s, e) => MoveSelected(_servingCombo, false);

            _countLabel = CrewLabel("");
            _countLabel.Width = 220;

            flow.Controls.AddRange(new Control[] { lbl1, _preparingCombo, ready, lbl2, _servingCombo, claimed, _countLabel });
            bar.Controls.Add(flow);
            return bar;
        }

        private static Label CrewLabel(string text)
        {
            return new Label
            {
                Text = text, ForeColor = Color.White, AutoSize = false, Width = 100, Height = 40,
                TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
        }

        private static ComboBox CrewCombo()
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, Width = 100,
                Font = new Font("Segoe UI", 13f), Margin = new Padding(3, 4, 3, 3)
            };
        }

        private static Button CrewButton(string text, Color back, Color fore)
        {
            return new Button
            {
                Text = text, Width = 140, Height = 40, FlatStyle = FlatStyle.Flat,
                BackColor = back, ForeColor = fore, Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
        }

        // ================= LOGIC =================

        private void RefreshQueue()
        {
            _clockLabel.Text = DateTime.Now.ToString("hh:mm:ss tt");

            QueueSnapshot snap;
            try
            {
                snap = _queue.GetSnapshot();
            }
            catch (Exception ex)
            {
                _countLabel.Text = "Read error: " + ex.Message;
                return;
            }

            if (snap.Signature == _lastSignature) return; // nothing changed, skip redraw
            _lastSignature = snap.Signature;

            // numbers on the monitor
            FillNumbers(_preparingPanel, snap.Preparing, 26f, Navy, 120);
            FillNumbers(_servingPanel, snap.NowServing, 54f, Red, 220);

            // crew dropdowns (keep current selection if still there)
            FillCombo(_preparingCombo, snap.Preparing);
            FillCombo(_servingCombo, snap.NowServing);
            _countLabel.Text = string.Format("Preparing: {0}   Serving: {1}", snap.Preparing.Count, snap.NowServing.Count);

            if (snap.NewlyServing.Count > 0)
            {
                SystemSounds.Exclamation.Play();
                _blinkTicks = 8;
                _blinkTimer.Start();
            }
        }

        private void FillNumbers(FlowLayoutPanel panel, System.Collections.Generic.List<int> numbers, float size, Color color, int width)
        {
            panel.SuspendLayout();
            foreach (Control c in panel.Controls) c.Dispose();
            panel.Controls.Clear();
            foreach (int n in numbers)
            {
                panel.Controls.Add(new Label
                {
                    Text = n.ToString(), ForeColor = color, AutoSize = false,
                    Width = width, Height = (int)(size * 1.9f),
                    Font = new Font("Segoe UI", size, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter, Tag = n
                });
            }
            panel.ResumeLayout();
        }

        private static void FillCombo(ComboBox combo, System.Collections.Generic.List<int> numbers)
        {
            object previous = combo.SelectedItem;
            combo.BeginUpdate();
            combo.Items.Clear();
            foreach (int n in numbers) combo.Items.Add(n);
            combo.EndUpdate();

            if (previous != null && combo.Items.Contains(previous)) combo.SelectedItem = previous;
            else if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private void MoveSelected(ComboBox combo, bool toServing)
        {
            if (combo.SelectedItem == null)
            {
                MessageBox.Show("No order selected.", "Claim Monitor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int number = (int)combo.SelectedItem;
            bool ok = toServing ? _queue.MarkReady(number) : _queue.MarkClaimed(number);
            if (!ok)
                MessageBox.Show("Order " + number + " could not be updated (it may have changed already).",
                    "Claim Monitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            combo.SelectedItem = null;
            RefreshQueue();
        }

        /// <summary>Flashes the "Now Serving" column when a new number is called.</summary>
        private void Blink()
        {
            _servingPanel.BackColor = (_blinkTicks % 2 == 0) ? Color.FromArgb(255, 235, 150) : Sky;
            _blinkTicks--;
            if (_blinkTicks <= 0)
            {
                _blinkTimer.Stop();
                _servingPanel.BackColor = Sky;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) _crewBar.Visible = !_crewBar.Visible;       // customer view
            if (e.KeyCode == Keys.F11)                                            // full screen
            {
                bool full = FormBorderStyle != FormBorderStyle.None;
                FormBorderStyle = full ? FormBorderStyle.None : FormBorderStyle.Sizable;
                WindowState = full ? FormWindowState.Maximized : FormWindowState.Normal;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
            _blinkTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
