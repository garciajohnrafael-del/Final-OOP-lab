using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OOP10FEXLab.Core.Models;

namespace OOP10FEXLab.Kiosk
{
    /// <summary>
    /// A clickable menu tile (like the boxes on the reference kiosk).
    /// Inheritance: MealCard IS-A Panel, with its own drawing and a MealSelected event.
    /// </summary>
    public class MealCard : Panel
    {
        private static readonly Color Accent = Color.FromArgb(200, 16, 46);

        public Meal Meal { get; private set; }
        public event EventHandler<Meal> MealSelected;

        public MealCard(Meal meal)
        {
            Meal = meal;
            Width = 190;
            Height = 210;
            Margin = new Padding(8);
            BackColor = Color.White;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;

            var name = new Label
            {
                Text = meal.Name,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.TopCenter,
                Left = 5, Top = 100, Width = Width - 10, Height = 40
            };
            var inclusions = new Label
            {
                Text = meal.Inclusions,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.TopCenter,
                Left = 5, Top = 140, Width = Width - 10, Height = 32
            };
            var price = new Label
            {
                Text = "P " + meal.Price.ToString("N2"),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Accent,
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 5, Top = 174, Width = Width - 10, Height = 26
            };

            Controls.Add(name);
            Controls.Add(inclusions);
            Controls.Add(price);

            // clicking any part of the card selects the meal
            Click += RaiseSelected;
            foreach (Control c in Controls) { c.Click += RaiseSelected; c.Cursor = Cursors.Hand; }

            MouseEnter += (s, e) => BackColor = Color.FromArgb(255, 244, 228);
            MouseLeave += (s, e) => BackColor = Color.White;
        }

        private void RaiseSelected(object sender, EventArgs e)
        {
            EventHandler<Meal> handler = MealSelected;
            if (handler != null) handler(this, Meal);
        }

        /// <summary>Draws a simple "food plate" with the meal code since we have no photos.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var plate = new Rectangle((Width - 80) / 2, 12, 80, 80);
            using (var b = new SolidBrush(Color.FromArgb(255, 205, 0))) g.FillEllipse(b, plate);
            using (var p = new Pen(Accent, 3)) g.DrawEllipse(p, plate);

            using (var f = new Font("Segoe UI", 16f, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var tb = new SolidBrush(Accent))
            {
                g.DrawString(Meal.Code, f, tb, plate, sf);
            }

            using (var border = new Pen(Color.Gainsboro)) g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        }
    }
}
