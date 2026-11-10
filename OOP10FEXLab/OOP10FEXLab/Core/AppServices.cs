using System;
using System.IO;
using OOP10FEXLab.Core.Data;

namespace OOP10FEXLab.Core
{
    /// <summary>
    /// One place where the data sources are chosen. Both the Kiosk and the Queue Monitor use these.
    /// To switch to the professor's database, replace the Meals line with a DatabaseMealRepository.
    /// </summary>
    public static class AppServices
    {
        public static readonly string DataFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        public static IMealRepository Meals { get; private set; }
        public static ITransactionRepository Transactions { get; private set; }

        public static void Initialize()
        {
            // NOTEPAD version (default)
            Meals = new TextFileMealRepository(Path.Combine(DataFolder, "meals.txt"));

            // DATABASE version - uncomment and paste the connection code from Prof. Raymond:
            // Meals = new DatabaseMealRepository(
            //     () => new MySqlConnection("server=localhost;database=kiosk;uid=root;pwd=;"),
            //     "SELECT code, name, category, price, inclusions FROM meals");

            Transactions = new TextFileTransactionRepository(Path.Combine(DataFolder, "transactions.txt"));
        }
    }
}
