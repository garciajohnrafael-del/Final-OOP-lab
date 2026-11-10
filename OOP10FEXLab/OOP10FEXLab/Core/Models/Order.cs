using System;
using System.Collections.Generic;
using System.Linq;

namespace OOP10FEXLab.Core.Models
{
    public enum OrderStatus
    {
        Preparing,
        NowServing,
        Claimed
    }

    public enum OrderType
    {
        DineIn,
        TakeOut
    }

    /// <summary>One line of an order (e.g. 2x Chicken Meal).</summary>
    public class OrderLine
    {
        public string MealCode { get; private set; }
        public string MealName { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }

        public decimal Subtotal { get { return UnitPrice * Quantity; } }

        public OrderLine(string mealCode, string mealName, int quantity, decimal unitPrice)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be at least 1.");
            MealCode = mealCode;
            MealName = mealName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }

    /// <summary>A transaction record. Saved to transactions.txt by the kiosk, read by the queue monitor.</summary>
    public class Order
    {
        private readonly List<OrderLine> _lines = new List<OrderLine>();

        public int OrderNumber { get; set; }            // assigned by the repository when saved
        public DateTime CreatedAt { get; set; }
        public OrderType Type { get; set; }
        public string PaymentMethod { get; set; }
        public OrderStatus Status { get; set; }

        public IReadOnlyList<OrderLine> Lines { get { return _lines.AsReadOnly(); } }

        public decimal Total { get { return _lines.Sum(l => l.Subtotal); } }

        public Order()
        {
            CreatedAt = DateTime.Now;
            Status = OrderStatus.Preparing;
            PaymentMethod = "Cash";
        }

        public void AddLine(OrderLine line)
        {
            if (line == null) throw new ArgumentNullException("line");
            _lines.Add(line);
        }
    }
}
