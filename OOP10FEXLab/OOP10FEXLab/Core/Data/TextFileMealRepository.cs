using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OOP10FEXLab.Core.Models;

namespace OOP10FEXLab.Core.Data
{
    /// <summary>
    /// Reads package meals from a notepad file (Data\meals.txt).
    /// Format per line:  CODE|Name|Category|Price|Inclusions
    /// Lines starting with # are comments. Bad lines are skipped (and counted) instead of crashing the kiosk.
    /// </summary>
    public class TextFileMealRepository : IMealRepository
    {
        private readonly string _filePath;

        public int SkippedLines { get; private set; }

        public TextFileMealRepository(string filePath)
        {
            _filePath = filePath;
        }

        public List<Meal> GetAll()
        {
            var meals = new List<Meal>();
            var seenCodes = new HashSet<string>();
            SkippedLines = 0;

            if (!File.Exists(_filePath))
                throw new FileNotFoundException("Menu file not found: " + _filePath);

            foreach (string rawLine in File.ReadAllLines(_filePath))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                string[] parts = line.Split('|');
                decimal price;
                if (parts.Length < 4 ||
                    !decimal.TryParse(parts[3].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out price))
                {
                    SkippedLines++;
                    continue;
                }

                try
                {
                    string inclusions = parts.Length >= 5 ? parts[4] : "";
                    var meal = new Meal(parts[0], parts[1], parts[2], price, inclusions);

                    if (!seenCodes.Add(meal.Code)) { SkippedLines++; continue; } // duplicate code
                    meals.Add(meal);
                }
                catch (ArgumentException)
                {
                    SkippedLines++;
                }
            }

            return meals;
        }
    }
}
