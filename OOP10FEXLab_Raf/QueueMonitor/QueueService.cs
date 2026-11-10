using System;
using System.Collections.Generic;
using System.Linq;
using OOP10FEXLab.Core.Data;
using OOP10FEXLab.Core.Models;

namespace OOP10FEXLab.QueueMonitor
{
    /// <summary>
    /// Queue logic (Raf's part). Keeps the rules out of the form:
    ///   Preparing  -> NowServing  (crew: "Order Ready")
    ///   NowServing -> Claimed     (crew: "Claimed", number disappears from the monitor)
    /// Only today's orders are shown so old test data does not clutter the screen.
    /// </summary>
    public class QueueService
    {
        private readonly ITransactionRepository _transactions;
        private HashSet<int> _lastNowServing = new HashSet<int>();

        public QueueService(ITransactionRepository transactions)
        {
            if (transactions == null) throw new ArgumentNullException("transactions");
            _transactions = transactions;
        }

        public QueueSnapshot GetSnapshot()
        {
            List<Order> today = _transactions.GetAll()
                .Where(o => o.CreatedAt.Date == DateTime.Today)
                .OrderBy(o => o.OrderNumber)
                .ToList();

            var snapshot = new QueueSnapshot
            {
                Preparing = today.Where(o => o.Status == OrderStatus.Preparing).Select(o => o.OrderNumber).ToList(),
                NowServing = today.Where(o => o.Status == OrderStatus.NowServing).Select(o => o.OrderNumber).ToList()
            };

            // numbers that just moved to "Now Serving" since the last refresh (used for the chime/highlight)
            snapshot.NewlyServing = snapshot.NowServing.Where(n => !_lastNowServing.Contains(n)).ToList();
            _lastNowServing = new HashSet<int>(snapshot.NowServing);
            return snapshot;
        }

        public bool MarkReady(int orderNumber)
        {
            return MoveStatus(orderNumber, OrderStatus.Preparing, OrderStatus.NowServing);
        }

        public bool MarkClaimed(int orderNumber)
        {
            return MoveStatus(orderNumber, OrderStatus.NowServing, OrderStatus.Claimed);
        }

        /// <summary>Only allows the correct next step (no skipping from Preparing to Claimed).</summary>
        private bool MoveStatus(int orderNumber, OrderStatus expectedCurrent, OrderStatus next)
        {
            Order order = _transactions.GetAll().FirstOrDefault(o => o.OrderNumber == orderNumber);
            if (order == null || order.Status != expectedCurrent) return false;
            return _transactions.UpdateStatus(orderNumber, next);
        }
    }

    public class QueueSnapshot
    {
        public List<int> Preparing { get; set; }
        public List<int> NowServing { get; set; }
        public List<int> NewlyServing { get; set; }

        /// <summary>Used to skip redrawing when nothing changed (prevents flicker).</summary>
        public string Signature
        {
            get { return string.Join(",", Preparing) + "#" + string.Join(",", NowServing); }
        }
    }
}
