using System;
using System.Collections.Generic;
using System.Linq;
using OOP10FEXLab.Core.Models;

namespace OOP10FEXLab.Kiosk
{
    /// <summary>One meal in the cart with its quantity.</summary>
    public class CartItem
    {
        public Meal Meal { get; private set; }
        public int Quantity { get; internal set; }
        public decimal Subtotal { get { return Meal.Price * Quantity; } }

        public CartItem(Meal meal, int quantity)
        {
            Meal = meal;
            Quantity = quantity;
        }
    }

    /// <summary>
    /// Shopping cart of the kiosk (MJ's part).
    /// Encapsulation: the list is private; the form can only change it through these methods,
    /// so quantity rules (1..MaxQuantityPerItem) are always enforced.
    /// </summary>
    public class Cart
    {
        public const int MaxQuantityPerItem = 20;

        private readonly List<CartItem> _items = new List<CartItem>();

        /// <summary>Raised whenever the cart changes so the UI can refresh.</summary>
        public event EventHandler Changed;

        public IReadOnlyList<CartItem> Items { get { return _items.AsReadOnly(); } }
        public decimal Total { get { return _items.Sum(i => i.Subtotal); } }
        public int ItemCount { get { return _items.Sum(i => i.Quantity); } }
        public bool IsEmpty { get { return _items.Count == 0; } }

        /// <returns>false if the item is already at the max quantity.</returns>
        public bool Add(Meal meal)
        {
            if (meal == null) throw new ArgumentNullException("meal");

            CartItem existing = Find(meal.Code);
            if (existing != null)
            {
                if (existing.Quantity >= MaxQuantityPerItem) return false;
                existing.Quantity++;
            }
            else
            {
                _items.Add(new CartItem(meal, 1));
            }
            OnChanged();
            return true;
        }

        public bool Increase(string mealCode)
        {
            CartItem item = Find(mealCode);
            if (item == null || item.Quantity >= MaxQuantityPerItem) return false;
            item.Quantity++;
            OnChanged();
            return true;
        }

        /// <summary>Decreases quantity; removes the item when it reaches 0.</summary>
        public void Decrease(string mealCode)
        {
            CartItem item = Find(mealCode);
            if (item == null) return;
            item.Quantity--;
            if (item.Quantity <= 0) _items.Remove(item);
            OnChanged();
        }

        public void Remove(string mealCode)
        {
            CartItem item = Find(mealCode);
            if (item == null) return;
            _items.Remove(item);
            OnChanged();
        }

        public void Clear()
        {
            _items.Clear();
            OnChanged();
        }

        /// <summary>Converts the cart into an Order ready to be saved to the transactions record.</summary>
        public Order ToOrder(OrderType type, string paymentMethod)
        {
            if (IsEmpty) throw new InvalidOperationException("Cart is empty.");

            var order = new Order { Type = type, PaymentMethod = paymentMethod };
            foreach (CartItem i in _items)
                order.AddLine(new OrderLine(i.Meal.Code, i.Meal.Name, i.Quantity, i.Meal.Price));
            return order;
        }

        private CartItem Find(string mealCode)
        {
            return _items.FirstOrDefault(i => i.Meal.Code == mealCode);
        }

        protected virtual void OnChanged()
        {
            EventHandler handler = Changed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
