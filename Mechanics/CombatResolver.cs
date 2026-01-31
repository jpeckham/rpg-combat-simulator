using System;
using PalladiumRiftsCombatSim.Models;

namespace PalladiumRiftsCombatSim.Mechanics
{
    public static class CombatResolver
    {
        public static void ResolveAttack(Combatant attacker, Combatant defender, Weapon? weapon = null)
        {
            if (weapon == null) weapon = attacker.ActiveWeapon;

            if (weapon == null) 
            {
                weapon = new Weapon { Name = "Unarmed Strike", DamageDiceCount = 1, DamageDiceSides = 4, IsMDC = attacker.IsMDCCreature }; 
            }
            
            // Check Ammo
            if (weapon.MaxPayload < 1000) // Don't track "Unlimited" ammo weapons strictly to avoid spam or bugs, but checking VolleySize is good.
            {
                 if (weapon.CurrentAmmo < weapon.VolleySize)
                 {
                     Console.WriteLine($"  > CLICK. Weapon empty/jammed. (Ammo: {weapon.CurrentAmmo}/{weapon.MaxPayload})");
                     return;
                 }
                 weapon.CurrentAmmo -= weapon.VolleySize;
            }

            Console.WriteLine($"\nACTION: {attacker.Name} attacks {defender.Name} with {weapon.Name}!");
            if (weapon.MaxPayload < 1000) Console.WriteLine($"  > Ammo: {weapon.CurrentAmmo}/{weapon.MaxPayload} remaining.");

            // 1. Roll Strike
            int d20 = Die.Roll(20);
            int strikeTotal = d20 + attacker.BonusStrike;
            Console.WriteLine($"  Strike Roll: {d20} + {attacker.BonusStrike} = {strikeTotal}");

            // Natural 1 is a miss
            if (d20 == 1)
            {
                Console.WriteLine("  > Natural 1! AUTOMATIC MISS!");
                return;
            }

            // Check for Automatic Hit (Natural 20)? 
            // Palladium rules: Nat 20 is a Critical Strike (Double Damage), but can still be dodged unless it's a sneak attack or surprise.
            // We will assume standard combat face-to-face.

            // 2. Determine if Defender Defends
            bool attemptedDodge = false;
            int defenseTotal = 0;

            if (defender.ActionsLeft > 0)
            {
                // "Reasonable Chance" Logic
                // We assume average d20 roll is 10.5. 
                // If (10 + BonusDodge) > StrikeTotal, it's reasonable.
                // OR if Strike is very high (Natural 20 or > 25), maybe don't waste the action unless desperate?
                // User said: "50/50 chance or better".
                // Strike To Beat = strikeTotal.
                // Need (Roll + Bonus) >= strikeTotal.
                // Roll Needed = strikeTotal - Bonus.
                // If Roll Needed <= 11 (approx 50%), then Dodge.
                
                int rollNeeded = strikeTotal - defender.BonusDodge;
                bool reasonableChance = rollNeeded <= 11;
                
                // Always dodge if it's a crit (desperation) or if chance is good.
                // Also, Rifts logic: Always dodge if you can? 
                // But User specifically requested "Reasonable chance".
                
                if (reasonableChance || d20 == 20)
                {
                    defender.ActionsLeft--;
                    attemptedDodge = true;
                    int roll = Die.Roll(20);
                    defenseTotal = roll + defender.BonusDodge;
                    Console.WriteLine($"  {defender.Name} attempts Dodge (Actions Left: {defender.ActionsLeft}): Rolled {roll} + {defender.BonusDodge} = {defenseTotal}");
                }
                else
                {
                     Console.WriteLine($"  {defender.Name} decides NOT to Dodge (Odds too low).");
                }
            }
            else
            {
                Console.WriteLine($"  {defender.Name} has NO ACTIONS left to Dodge!");
            }

            // 3. Compare
            bool hit = true;
            if (attemptedDodge && defenseTotal >= strikeTotal)
            {
                Console.WriteLine("  > BLOCKED/DODGED! Attack missed.");
                hit = false;
                return; // Interaction ends
            }
            
            // 4. Resolve Hit & Damage
            if (hit)
            {
                // Check AR (Armor Rating) if applicable
                // Logic:
                // If Defender has AR (Natural or Armor):
                // Strike Roll (TOTAL) must be > AR to damage HP/MDC-Body.
                // If Strike Roll <= AR, damage goes to Armor (if SDC armor) or bounces off (if Natural AR check failed).
                
                // Rifts Logic for MDC Armor (e.g. Glitter Boy):
                // MDC Armor usually doesn't have an AR (it covers everything). 
                // Some Cyborg armor has AR.
                // If AR is present:
                int targetAR = (defender.BodyArmor != null) ? defender.BodyArmor.AR : defender.AR;
                
                // If target is wearing environmental MDC armor, AR is usually effectively 20 or not applicable (always hits armor).
                // If we treat AR 0 as "No AR / Full Coverage".
                
                bool penetratesAR = true;
                if (targetAR > 0)
                {
                    if (strikeTotal <= targetAR)
                    {
                        penetratesAR = false;
                        Console.WriteLine($"  > Attack Hit ({strikeTotal}), but failed to penetrate AR ({targetAR}). Damage to Armor.");
                    }
                    else
                    {
                        Console.WriteLine($"  > Attack Penetrated AR ({targetAR})!");
                    }
                }
                
                var damageResult = weapon.RollDamageDetailed();
                int damage = damageResult.total;
                string rollsStr = string.Join(", ", damageResult.rolls);
                
                // Formatted output
                int totalDice = weapon.DamageDiceCount * weapon.VolleySize;
                string volleyMsg = weapon.VolleySize > 1 ? $" (Volley of {weapon.VolleySize})" : "";
                Console.WriteLine($"  > Damage Roll{volleyMsg} ({totalDice}D{weapon.DamageDiceSides}{(weapon.Multiplier > 1 ? "x" + weapon.Multiplier : "")}): {damage} {(weapon.IsMDC ? "M.D." : "S.D.C.")} (Dice: {rollsStr})");

                if (!penetratesAR)
                {
                    if (defender.BodyArmor != null && defender.BodyArmor.AR >= strikeTotal)
                    {
                         // Hit the armor
                         if (weapon.IsMDC)
                         {
                             if (defender.BodyArmor.IsMDC) 
                             {
                                 defender.BodyArmor.MDC -= damage;
                                 Console.WriteLine($"  > Armor takes {damage} MD. Remaining Armor MDC: {defender.BodyArmor.MDC}");
                             }
                             else
                             {
                                 Console.WriteLine("  > SDC Armor vaporized by MDC hit!");
                                 defender.BodyArmor = null;
                             }
                         }
                         else // SDC Damage
                         {
                             if (defender.BodyArmor.IsMDC)
                             {
                                 Console.WriteLine("  > SDC Attack bounces off MDC Armor.");
                             }
                             else
                             {
                                 defender.BodyArmor.SDC -= damage;
                                 Console.WriteLine($"  > Armor takes {damage} SDC. Remaining Armor SDC: {defender.BodyArmor.SDC}");
                             }
                         }
                    }
                    else if (defender.AR >= strikeTotal)
                    {
                        Console.WriteLine("  > Attack bounces off Natural Armor (No Damage).");
                    }
                }
                else
                {
                    // Body/Armor normal hit
                    defender.TakeDamage(damage, weapon.IsMDC);
                    
                    // Report Status
                    if (defender.BodyArmor != null && defender.BodyArmor.MDC > 0)
                         Console.WriteLine($"  > Status: Armor MDC: {defender.BodyArmor.MDC}");
                    else if (defender.MDC > 0)
                         Console.WriteLine($"  > Status: Body MDC: {defender.MDC}");
                    else if (defender.IsDestroyed)
                         Console.WriteLine($"  > Status: DESTROYED");
                    else
                         Console.WriteLine($"  > Status: SDC: {defender.SDC}, HP: {defender.HitPoints}");
                }
            }
        }
    }
}
