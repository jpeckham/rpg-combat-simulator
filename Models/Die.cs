using System;

namespace PalladiumRiftsCombatSim.Models
{
    public static class Die
    {
        private static readonly Random _random = new Random();

        public static int Roll(int sides, int count = 1)
        {
            if (sides < 1) throw new ArgumentException("Sides must be at least 1.");
            if (count < 1) throw new ArgumentException("Count must be at least 1.");

            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total += _random.Next(1, sides + 1);
            }
            return total;
        }

        public static int RollPercent()
        {
            return Roll(100);
        }

        public static (int total, List<int> rolls) RollDetailed(int sides, int count = 1)
        {
            if (sides < 1) throw new ArgumentException("Sides must be at least 1.");
            if (count < 1) throw new ArgumentException("Count must be at least 1.");

            var rolls = new List<int>();
            int total = 0;
            for (int i = 0; i < count; i++)
            {
                int r = _random.Next(1, sides + 1);
                rolls.Add(r);
                total += r;
            }
            return (total, rolls);
        }
    }
}
