using System;

namespace PalladiumRiftsCombatSim.Models
{
    public abstract class Equipment
    {
        public string Name { get; set; } = "Unnamed Equipment";
    }

    public class Weapon : Equipment
    {
        public int DamageDiceCount { get; set; }
        public int DamageDiceSides { get; set; }
        public int DamageBonus { get; set; }
        public int Multiplier { get; set; } = 1; // e.g. x10
        public bool IsMDC { get; set; }
        
        public int MaxPayload { get; set; } = 1000; // Default unlimited-ish
        public int CurrentAmmo { get; set; } = 1000;
        public int VolleySize { get; set; } = 1; // Shots per attack
        public int MaxVolleySize { get; set; } = 1; // Limit for launcher

        public int RollDamage()
        {
            // Simple roll (deprecated by Detailed)
            return (Die.Roll(DamageDiceSides, DamageDiceCount) * Multiplier * VolleySize) + DamageBonus;
        }

        public (int total, List<int> rolls) RollDamageDetailed()
        {
            // For a volley, we roll the base damage ONCE and multiply by VolleySize? 
            // OR do we roll independently for each missile?
            // Rifts Rules: Usually one roll for the volley, or roll once for missile damage and multiply.
            // "Volley of 3 missiles" -> 3 * (Roll).
            // Let's roll ONCE and multiply total by VolleySize for simplicity/speed, 
            // OR roll (DiceCount * VolleySize) dice.
            // Let's roll (DiceCount * VolleySize) to be statistically accurate to "3 missiles hitting".
            
            // Wait, Multiplier is for "x10" MD.
            // Volley is "3 missiles".
            // So if 1 missile is 2D6x10.
            // 3 missiles should be 6D6x10.
            
            int actualDiceCount = DamageDiceCount * VolleySize;
            
            var result = Die.RollDetailed(DamageDiceSides, actualDiceCount);
            int totalDamage = (result.total * Multiplier) + DamageBonus;
            return (totalDamage, result.rolls);
        }

        public override string ToString()
        {
            string damageType = IsMDC ? "M.D." : "S.D.C.";
            string multStr = Multiplier > 1 ? $"x{Multiplier}" : "";
            string bonusStr = DamageBonus != 0 ? $"+{DamageBonus}" : "";
            return $"{Name} ({DamageDiceCount}D{DamageDiceSides}{multStr}{bonusStr} {damageType}) [Ammo: {CurrentAmmo}/{MaxPayload}]";
        }
    }

    public class Armor : Equipment
    {
        public int MDC { get; set; }
        public int SDC { get; set; }
        public int AR { get; set; } // Armor Rating (for SDC armor)
        public bool IsMDC { get; set; }

        public Armor(string name, int mdc, int sdc, int ar, bool isMdc)
        {
            Name = name;
            MDC = mdc;
            SDC = sdc;
            AR = ar;
            IsMDC = isMdc;
        }
    }
}
