using System;

namespace OOP10FEXLab.Core.Models
{
    /// <summary>
    /// A package meal loaded from meals.txt (or the database).
    /// Encapsulation: properties are read-only from outside; the constructor validates the data.
    /// </summary>
    public class Meal
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public string Category { get; private set; }
        public decimal Price { get; private set; }
        public string Inclusions { get; private set; }

        public Meal(string code, string name, string category, decimal price, string inclusions)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Meal code is required.");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Meal name is required.");
            if (price <= 0) throw new ArgumentException("Price must be greater than zero.");

            Code = code.Trim().ToUpperInvariant();
            Name = name.Trim();
            Category = string.IsNullOrWhiteSpace(category) ? "Others" : category.Trim();
            Price = price;
            Inclusions = inclusions == null ? "" : inclusions.Trim();
        }

        public override string ToString()
        {
            return string.Format("{0} - P{1:N2}", Name, Price);
        }
    }
}
