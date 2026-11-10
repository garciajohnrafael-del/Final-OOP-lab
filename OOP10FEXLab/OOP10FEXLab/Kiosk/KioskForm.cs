using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OOP10FEXLab.Core;
using OOP10FEXLab.Core.Models;

namespace OOP10FEXLab.Kiosk
{
    /// <summary>
    /// SELF-ORDER KIOSK (MJ's part)
    /// Flow: Welcome (Dine In / Take Out) -> Menu + Cart -> Payment -> Order Number.
    /// Meals come from AppServices.Meals (meals.txt or DB); the finished order is saved to
    /// AppServices.Transactions (transactions.txt), where the Queue Monitor picks it up.
    /// </summary>
    public class KioskForm : Form
    {
        private static readonly Color Red = Color.FromArgb(200, 16, 46);
        private static readonly Color Yellow = Color.FromArgb(255, 205, 0);
        private static readonly Color Cream = Color.FromArgb(250, 246, 240);

        private readonly Cart _cart = new Cart();
        private List<Meal> _meals = new List<Meal>();
        private OrderType _orderType = OrderType.DineIn;

        // screens
        private Panel _welcomeScreen, _menuScreen, _paymentScreen, _doneScreen;

        // menu screen controls
        private FlowLayoutPanel _categoryPanel, _mealGrid;
        private Label _orderTypeLabel, _categoryTitle;
        private ListView _cartList;
        private Label _totalLabel;
        private Button _payButton;

        // payment + done screens
        private ListBox _summaryList;
        private Label _paymentTotalLabel, _orderNumberLabel;
        private readonly Timer _resetTimer = new Timer { Interval = 10000 };

        public KioskForm()
        {
            Text = "QuickBite - Self-Order Kiosk";
            Font = new Font("Segoe UI", 10f);
            MinimumSize = new Size(1000, 650);
            Size = new Size(1200, 760);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Cream;

            BuildWelcomeScreen();
            BuildMenuScreen();
            BuildPaymentScreen();
            BuildDoneScreen();

            _cart.Changed += (s, e) => RefreshCart();
            _resetTimer.Tick += (s, e) => StartOver();

            LoadMeals();
            ShowScreen(_welcomeScreen);
        }

        // ================= DATA =================

        private void LoadMeals()
        {
            try
            {
                _meals = AppServices.Meals.GetAll();
                if (_meals.Count == 0)
                    MessageBox.Show("No meals were found in the menu source.", "Menu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                _meals = new List<Meal>();
                MessageBox.Show("Could not load the menu:\n" + ex.Message, "Menu Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            BuildCategoryButtons();
            ShowCategory("All");
        }

        // ================= SCREEN 1: WELCOME =================

        private void BuildWelcomeScreen()
        {
            _welcomeScreen = new Panel { Dock = DockStyle.Fill, BackColor = Red };

            var title = new Label
            {
                Text = "Welcome to QuickBite!",
                Font = new Font("Segoe UI", 34f, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top, Height = 220,
                TextAlign = ContentAlignment.BottomCenter
            };
            var subtitle = new Label
            {
                Text = "Order and Pay here",
                Font = new Font("Segoe UI", 18f),
                ForeColor = Yellow,
                Dock = DockStyle.Top, Height = 60,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 200,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 30, 0, 0)
            };
            Button dineIn = BigButton("DINE IN", Yellow, Color.Black);
            Button takeOut = BigButton("TAKE OUT", Color.White, Red);
            dineIn.Click += (s, e) => StartOrder(OrderType.DineIn);
            takeOut.Click += (s, e) => StartOrder(OrderType.TakeOut);
            buttons.Controls.Add(dineIn);
            buttons.Controls.Add(takeOut);
            // center the two buttons
            buttons.Resize += (s, e) =>
            {
                int left = Math.Max(0, (buttons.Width - 2 * (dineIn.Width + 20)) / 2);
                buttons.Padding = new Padding(left, 30, 0, 0);
            };

            _welcomeScreen.Controls.Add(buttons);
            _welcomeScreen.Controls.Add(subtitle);
            _welcomeScreen.Controls.Add(title);
            Controls.Add(_welcomeScreen);
        }

        private void StartOrder(OrderType type)
        {
            _orderType = type;
            _orderTypeLabel.Text = type == OrderType.DineIn ? "DINE IN" : "TAKE OUT";
            _cart.Clear();
            ShowCategory("All");
            ShowScreen(_menuScreen);
        }

        // ================= SCREEN 2: MENU + CART =================

        private void BuildMenuScreen()
        {
            _menuScreen = new Panel { Dock = DockStyle.Fill, BackColor = Cream };

            // --- center: meal grid (added first because it is Dock.Fill) ---
            var center = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            _mealGrid = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Cream };
            _categoryTitle = new Label
            {
                Dock = DockStyle.Top, Height = 40,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Red
            };
            center.Controls.Add(_mealGrid);
            center.Controls.Add(_categoryTitle);

            // --- left: categories ---
            _categoryPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Left, Width = 170,
                FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoScroll = true, BackColor = Color.White,
                Padding = new Padding(8)
            };

            // --- right: cart ---
            Panel cartPanel = BuildCartPanel();

            // --- top banner ---
            var banner = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Red };
            var brand = new Label
            {
                Text = "QuickBite", ForeColor = Color.White,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                Dock = DockStyle.Left, Width = 250, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            _orderTypeLabel = new Label
            {
                ForeColor = Yellow, Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Dock = DockStyle.Right, Width = 200, TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 15, 0)
            };
            banner.Controls.Add(brand);
            banner.Controls.Add(_orderTypeLabel);

            _menuScreen.Controls.Add(center);
            _menuScreen.Controls.Add(_categoryPanel);
            _menuScreen.Controls.Add(cartPanel);
            _menuScreen.Controls.Add(banner);
            Controls.Add(_menuScreen);
        }

        private Panel BuildCartPanel()
        {
            var panel = new Panel { Dock = DockStyle.Right, Width = 340, BackColor = Color.White, Padding = new Padding(10) };

            var title = new Label
            {
                Text = "My Order", Dock = DockStyle.Top, Height = 36,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Red
            };

            _cartList = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
                MultiSelect = false, HideSelection = false, Font = new Font("Segoe UI", 10f)
            };
            _cartList.Columns.Add("Item", 160);
            _cartList.Columns.Add("Qty", 45, HorizontalAlignment.Center);
            _cartList.Columns.Add("Subtotal", 95, HorizontalAlignment.Right);

            // + / - / remove buttons for the selected cart row
            var qtyRow = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(0, 6, 0, 0) };
            Button plus = SmallButton("+");
            Button minus = SmallButton("-");
            Button remove = SmallButton("Remove");
            remove.Width = 120;
            plus.Click += (s, e) =>
            {
                string code = SelectedCartCode();
                if (code == null) { Hint("Select an item in your order first."); return; }
                if (!_cart.Increase(code)) Hint("Maximum of " + Cart.MaxQuantityPerItem + " per item.");
                Reselect(code);
            };
            minus.Click += (s, e) =>
            {
                string code = SelectedCartCode();
                if (code == null) { Hint("Select an item in your order first."); return; }
                _cart.Decrease(code);
                Reselect(code);
            };
            remove.Click += (s, e) =>
            {
                string code = SelectedCartCode();
                if (code == null) { Hint("Select an item in your order first."); return; }
                _cart.Remove(code);
            };
            qtyRow.Controls.Add(minus);
            qtyRow.Controls.Add(plus);
            qtyRow.Controls.Add(remove);

            _totalLabel = new Label
            {
                Dock = DockStyle.Bottom, Height = 44, TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Red
            };

            var actions = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 58, ColumnCount = 2 };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            var cancel = new Button
            {
                Text = "Cancel", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat,
                BackColor = Color.Gainsboro, Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };
            _payButton = new Button
            {
                Text = "Review && Pay", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat,
                BackColor = Red, ForeColor = Color.White, Font = new Font("Segoe UI", 12f, FontStyle.Bold)
            };
            cancel.Click += (s, e) => CancelOrder();
            _payButton.Click += (s, e) => GoToPayment();
            actions.Controls.Add(cancel, 0, 0);
            actions.Controls.Add(_payButton, 1, 0);

            panel.Controls.Add(_cartList);
            panel.Controls.Add(qtyRow);
            panel.Controls.Add(_totalLabel);
            panel.Controls.Add(actions);
            panel.Controls.Add(title);
            return panel;
        }

        private void BuildCategoryButtons()
        {
            _categoryPanel.Controls.Clear();
            var categories = new List<string> { "All" };
            categories.AddRange(_meals.Select(m => m.Category).Distinct());

            foreach (string cat in categories)
            {
                string category = cat; // capture for the lambda
                var b = new Button
                {
                    Text = category, Width = 145, Height = 50,
                    FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    Tag = category, Cursor = Cursors.Hand
                };
                b.FlatAppearance.BorderColor = Color.Gainsboro;
                b.Click += (s, e) => ShowCategory(category);
                _categoryPanel.Controls.Add(b);
            }
        }

        private void ShowCategory(string category)
        {
            _categoryTitle.Text = category == "All" ? "All Meals" : category;

            foreach (Control c in _categoryPanel.Controls)
            {
                bool active = (string)c.Tag == category;
                c.BackColor = active ? Yellow : Color.White;
            }

            _mealGrid.SuspendLayout();
            foreach (Control old in _mealGrid.Controls) old.Dispose();
            _mealGrid.Controls.Clear();

            IEnumerable<Meal> list = category == "All" ? _meals : _meals.Where(m => m.Category == category);
            foreach (Meal meal in list)
            {
                var card = new MealCard(meal);
                card.MealSelected += (s, m) =>
                {
                    if (!_cart.Add(m)) Hint("Maximum of " + Cart.MaxQuantityPerItem + " per item.");
                };
                _mealGrid.Controls.Add(card);
            }
            _mealGrid.ResumeLayout();
        }

        private void RefreshCart()
        {
            _cartList.BeginUpdate();
            _cartList.Items.Clear();
            foreach (CartItem item in _cart.Items)
            {
                var row = new ListViewItem(item.Meal.Name) { Tag = item.Meal.Code };
                row.SubItems.Add(item.Quantity.ToString());
                row.SubItems.Add(item.Subtotal.ToString("N2"));
                _cartList.Items.Add(row);
            }
            _cartList.EndUpdate();

            _totalLabel.Text = "Total: P " + _cart.Total.ToString("N2");
            _payButton.Enabled = !_cart.IsEmpty;
        }

        private string SelectedCartCode()
        {
            return _cartList.SelectedItems.Count == 0 ? null : (string)_cartList.SelectedItems[0].Tag;
        }

        private void Reselect(string code)
        {
            foreach (ListViewItem row in _cartList.Items)
                if ((string)row.Tag == code) { row.Selected = true; row.Focused = true; }
        }

        private void CancelOrder()
        {
            if (!_cart.IsEmpty &&
                MessageBox.Show("Cancel this order and start over?", "Cancel Order",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            StartOver();
        }

        // ================= SCREEN 3: PAYMENT =================

        private void BuildPaymentScreen()
        {
            _paymentScreen = new Panel { Dock = DockStyle.Fill, BackColor = Cream, Padding = new Padding(40, 20, 40, 20) };

            var title = new Label
            {
                Text = "Review your order", Dock = DockStyle.Top, Height = 60,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold), ForeColor = Red
            };

            _summaryList = new ListBox
            {
                Dock = DockStyle.Fill, Font = new Font("Consolas", 12f),
                BorderStyle = BorderStyle.None, IntegralHeight = false
            };

            _paymentTotalLabel = new Label
            {
                Dock = DockStyle.Bottom, Height = 60, TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold), ForeColor = Red
            };

            var pick = new Label
            {
                Text = "Choose payment method:", Dock = DockStyle.Bottom, Height = 36,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold)
            };

            var methods = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 90 };
            foreach (string method in new[] { "Cash (pay at counter)", "Card", "E-Wallet" })
            {
                string m = method;
                var b = new Button
                {
                    Text = m, Width = 240, Height = 70, FlatStyle = FlatStyle.Flat,
                    BackColor = Yellow, Font = new Font("Segoe UI", 12f, FontStyle.Bold), Cursor = Cursors.Hand
                };
                b.Click += (s, e) => PlaceOrder(m);
                methods.Controls.Add(b);
            }

            var back = new Button
            {
                Text = "< Back to Menu", Dock = DockStyle.Bottom, Height = 45, FlatStyle = FlatStyle.Flat,
                BackColor = Color.Gainsboro, Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };
            back.Click += (s, e) => ShowScreen(_menuScreen);

            _paymentScreen.Controls.Add(_summaryList);
            _paymentScreen.Controls.Add(_paymentTotalLabel);
            _paymentScreen.Controls.Add(pick);
            _paymentScreen.Controls.Add(methods);
            _paymentScreen.Controls.Add(back);
            _paymentScreen.Controls.Add(title);
            Controls.Add(_paymentScreen);
        }

        private void GoToPayment()
        {
            if (_cart.IsEmpty) { Hint("Your order is empty. Tap a meal to add it."); return; }

            _summaryList.Items.Clear();
            _summaryList.Items.Add(_orderType == OrderType.DineIn ? "DINE IN" : "TAKE OUT");
            _summaryList.Items.Add(new string('-', 52));
            foreach (CartItem i in _cart.Items)
                _summaryList.Items.Add(string.Format("{0,3}x {1,-32} {2,12:N2}", i.Quantity, Truncate(i.Meal.Name, 32), i.Subtotal));
            _paymentTotalLabel.Text = "TOTAL: P " + _cart.Total.ToString("N2");
            ShowScreen(_paymentScreen);
        }

        private void PlaceOrder(string paymentMethod)
        {
            try
            {
                Order order = _cart.ToOrder(_orderType, paymentMethod);
                int number = AppServices.Transactions.Save(order);   // <- saved to transactions record
                _orderNumberLabel.Text = number.ToString();
                _cart.Clear();
                ShowScreen(_doneScreen);
                _resetTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Order was NOT saved:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= SCREEN 4: ORDER NUMBER =================

        private void BuildDoneScreen()
        {
            _doneScreen = new Panel { Dock = DockStyle.Fill, BackColor = Red };

            var thanks = new Label
            {
                Text = "Thank you! Your order number is", Dock = DockStyle.Top, Height = 180,
                TextAlign = ContentAlignment.BottomCenter, ForeColor = Color.White,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold)
            };
            _orderNumberLabel = new Label
            {
                Dock = DockStyle.Top, Height = 170, TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Yellow, Font = new Font("Segoe UI", 80f, FontStyle.Bold)
            };
            var info = new Label
            {
                Text = "Please wait for your number on the Claim Monitor.\nCash payments: please pay at the counter.",
                Dock = DockStyle.Top, Height = 90, TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White, Font = new Font("Segoe UI", 14f)
            };
            var newOrder = new Button
            {
                Text = "Start New Order", Dock = DockStyle.Bottom, Height = 70, FlatStyle = FlatStyle.Flat,
                BackColor = Yellow, Font = new Font("Segoe UI", 16f, FontStyle.Bold)
            };
            newOrder.Click += (s, e) => StartOver();

            _doneScreen.Controls.Add(info);
            _doneScreen.Controls.Add(_orderNumberLabel);
            _doneScreen.Controls.Add(thanks);
            _doneScreen.Controls.Add(newOrder);
            Controls.Add(_doneScreen);
        }

        // ================= HELPERS =================

        private void StartOver()
        {
            _resetTimer.Stop();
            _cart.Clear();
            ShowScreen(_welcomeScreen);
        }

        private void ShowScreen(Panel screen)
        {
            foreach (Panel p in new[] { _welcomeScreen, _menuScreen, _paymentScreen, _doneScreen })
                p.Visible = p == screen;
            screen.BringToFront();
        }

        private void Hint(string message)
        {
            MessageBox.Show(message, "Kiosk", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static Button BigButton(string text, Color back, Color fore)
        {
            return new Button
            {
                Text = text, Width = 280, Height = 140, Margin = new Padding(10),
                FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = fore,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold), Cursor = Cursors.Hand
            };
        }

        private static Button SmallButton(string text)
        {
            return new Button
            {
                Text = text, Width = 60, Height = 38, FlatStyle = FlatStyle.Flat,
                BackColor = Yellow, Font = new Font("Segoe UI", 12f, FontStyle.Bold)
            };
        }

        private static string Truncate(string s, int max)
        {
            return s.Length <= max ? s : s.Substring(0, max - 1) + ".";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _resetTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
