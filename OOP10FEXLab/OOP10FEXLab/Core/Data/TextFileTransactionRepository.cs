using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using OOP10FEXLab.Core.Models;

namespace OOP10FEXLab.Core.Data
{
    /// <summary>
    /// Transactions record saved in a notepad file (Data\transactions.txt).
    /// One order per line:
    ///   OrderNo|yyyy-MM-dd HH:mm:ss|DineIn|PaymentMethod|Status|Total|CODE^Name^Qty^UnitPrice;CODE^Name^Qty^UnitPrice
    ///
    /// The Kiosk writes new orders here; the Queue Monitor reads and updates the Status column.
    /// </summary>
    public class TextFileTransactionRepository : ITransactionRepository
    {
        public const int FirstOrderNumber = 1001;
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        // Both screens run in the same app, so one lock keeps them from writing at the same time.
        private static readonly object FileLock = new object();

        private readonly string _filePath;

        public TextFileTransactionRepository(string filePath)
        {
            _filePath = filePath;
            string folder = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
        }

        public int Save(Order order)
        {
            if (order == null) throw new ArgumentNullException("order");
            if (order.Lines.Count == 0) throw new InvalidOperationException("Cannot save an empty order.");

            lock (FileLock)
            {
                List<Order> existing = ReadAllUnsafe();
                order.OrderNumber = existing.Count == 0
                    ? FirstOrderNumber
                    : existing.Max(o => o.OrderNumber) + 1;
                order.Status = OrderStatus.Preparing;

                WithRetry(() => File.AppendAllText(_filePath, Serialize(order) + Environment.NewLine, Encoding.UTF8));
                return order.OrderNumber;
            }
        }

        public List<Order> GetAll()
        {
            lock (FileLock)
            {
                return ReadAllUnsafe();
            }
        }

        public bool UpdateStatus(int orderNumber, OrderStatus newStatus)
        {
            lock (FileLock)
            {
                List<Order> orders = ReadAllUnsafe();
                Order target = orders.FirstOrDefault(o => o.OrderNumber == orderNumber);
                if (target == null) return false;

                target.Status = newStatus;
                string[] lines = orders.Select(Serialize).ToArray();
                WithRetry(() => File.WriteAllLines(_filePath, lines, Encoding.UTF8));
                return true;
            }
        }

        // ---------- helpers ----------

        private List<Order> ReadAllUnsafe()
        {
            var orders = new List<Order>();
            if (!File.Exists(_filePath)) return orders;

            string[] lines = null;
            WithRetry(() => lines = File.ReadAllLines(_filePath, Encoding.UTF8));

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                Order o = TryParse(line);
                if (o != null) orders.Add(o);
            }
            return orders;
        }

        private static string Serialize(Order o)
        {
            string items = string.Join(";", o.Lines.Select(l => string.Join("^",
                Clean(l.MealCode), Clean(l.MealName),
                l.Quantity.ToString(CultureInfo.InvariantCulture),
                l.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture))));

            return string.Join("|",
                o.OrderNumber.ToString(CultureInfo.InvariantCulture),
                o.CreatedAt.ToString(DateFormat, CultureInfo.InvariantCulture),
                o.Type.ToString(),
                Clean(o.PaymentMethod),
                o.Status.ToString(),
                o.Total.ToString("0.00", CultureInfo.InvariantCulture),
                items);
        }

        private static Order TryParse(string line)
        {
            try
            {
                string[] p = line.Split('|');
                if (p.Length < 7) return null;

                var order = new Order
                {
                    OrderNumber = int.Parse(p[0], CultureInfo.InvariantCulture),
                    CreatedAt = DateTime.ParseExact(p[1], DateFormat, CultureInfo.InvariantCulture),
                    Type = (OrderType)Enum.Parse(typeof(OrderType), p[2]),
                    PaymentMethod = p[3],
                    Status = (OrderStatus)Enum.Parse(typeof(OrderStatus), p[4])
                };

                foreach (string item in p[6].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] f = item.Split('^');
                    if (f.Length < 4) continue;
                    order.AddLine(new OrderLine(f[0], f[1],
                        int.Parse(f[2], CultureInfo.InvariantCulture),
                        decimal.Parse(f[3], CultureInfo.InvariantCulture)));
                }
                return order;
            }
            catch (Exception)
            {
                return null; // corrupted line: skip it, don't crash the monitor
            }
        }

        /// <summary>Removes characters used as separators in the file.</summary>
        private static string Clean(string s)
        {
            if (s == null) return "";
            return s.Replace("|", "/").Replace(";", ",").Replace("^", " ").Replace("\r", " ").Replace("\n", " ");
        }

        /// <summary>Retries file access in case the other screen/app has the file open for a moment.</summary>
        private static void WithRetry(Action action)
        {
            for (int attempt = 1; ; attempt++)
            {
                try { action(); return; }
                catch (IOException)
                {
                    if (attempt >= 5) throw;
                    Thread.Sleep(100);
                }
            }
        }
    }
}
