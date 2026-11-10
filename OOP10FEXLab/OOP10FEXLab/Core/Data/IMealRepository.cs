using System.Collections.Generic;
using OOP10FEXLab.Core.Models;

namespace OOP10FEXLab.Core.Data
{
    /// <summary>
    /// Abstraction: the kiosk only knows "something gives me meals".
    /// Polymorphism: TextFileMealRepository and DatabaseMealRepository both implement this,
    /// so we can switch data source in ONE line (see AppServices.cs).
    /// </summary>
    public interface IMealRepository
    {
        List<Meal> GetAll();
    }

    public interface ITransactionRepository
    {
        /// <summary>Saves a new order and returns the order number assigned to it.</summary>
        int Save(Order order);
        List<Order> GetAll();
        bool UpdateStatus(int orderNumber, OrderStatus newStatus);
    }
}
