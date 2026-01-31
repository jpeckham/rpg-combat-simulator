using System;
using System.Collections.Generic;

namespace PalladiumRiftsCombatSim.Models
{
    public class Combatant
    {
        public string Name { get; set; }
        public int MDC { get; set; } // Mega-Damage Capacity (0 if SDC creature)
        public int SDC { get; set; } // Structural Damage Capacity
        public int HitPoints { get; set; }
        public int AR { get; set; } // Natural AR or Body Armor AR

        public int AttacksPerMelee { get; set; }
        public int ActionsLeft { get; set; } // Tracks AP during a round
        
        // Bonuses
        public int BonusStrike { get; set; }
        public int BonusParry { get; set; }
        public int BonusDodge { get; set; }
        public int BonusRollWithPunch { get; set; }
        public int BonusInitiative { get; set; }

        // Equipment
        public List<Weapon> Weapons { get; set; } = new List<Weapon>();
        
        private Weapon? _activeWeapon;
        public Weapon? ActiveWeapon 
        { 
            get 
            {
                if (_activeWeapon == null || _activeWeapon.CurrentAmmo < _activeWeapon.VolleySize)
                {
                    _activeWeapon = SelectBestWeapon();
                }
                return _activeWeapon;
            }
            set { _activeWeapon = value; }
        }

        public Armor? BodyArmor { get; set; }

        public bool IsDestroyed 
        { 
             get 
             {
                 if (MDC > 0) return false; 
                 if (IsMDCCreature) return MDC <= 0;
                 return HitPoints <= 0;
             }
        }

        public bool IsMDCCreature { get; set; } 

        public Combatant(string name, bool isMdcCreature = false)
        {
            Name = name;
            IsMDCCreature = isMdcCreature;
            AttacksPerMelee = 2; 
        }

        public Weapon? SelectBestWeapon()
        {
            // Pick first weapon with ANY ammo.
            foreach(var w in Weapons)
            {
                if (w.CurrentAmmo > 0) return w;
            }
            return null; 
        }

        public void ConfigureAttack(Combatant target)
        {
            // Ensure ActiveWeapon is up to date
             if (ActiveWeapon == null || ActiveWeapon.CurrentAmmo <= 0)
            {
                ActiveWeapon = SelectBestWeapon();
            }

            var w = ActiveWeapon;
            if (w == null) return;

            // Dynamic Volley Logic
            if (w.MaxVolleySize > 1) 
            {
                // If target cannot dodge (ActionsLeft <= 0), UNLEASH HELL (Max Volley)
                if (target.ActionsLeft <= 0)
                {
                     w.VolleySize = Math.Min(w.MaxVolleySize, w.CurrentAmmo);
                     // Console.WriteLine($"  [Tactical] {Name} sees target vulnerable! Adjusting volley to {w.VolleySize}!");
                }
                else
                {
                    // Conservative: 2 missiles (or 1 if only 1 left)
                    w.VolleySize = Math.Min(2, w.CurrentAmmo);
                }
                
                // Safety clamp
                if (w.VolleySize < 1) w.VolleySize = 1;
            }
        }

        public void TakeDamage(int damage, bool isMdcDamage)
        {
            if (isMdcDamage)
            {
                if (IsMDCCreature || (BodyArmor != null && BodyArmor.IsMDC && BodyArmor.MDC > 0))
                {
                    // Absorb with Armor first if MDC armor
                    if (BodyArmor != null && BodyArmor.IsMDC && BodyArmor.MDC > 0)
                    {
                        BodyArmor.MDC -= damage;
                        if (BodyArmor.MDC < 0)
                        {
                            // Bleed through to main body? 
                            // Rifts rules generally destroy the armor first, remainder usually lost or applies to body depending on interpretation.
                            // For simplicity, we'll say armor absorbs it all until it breaks, remainder is lost unless it's a huge overkill, but lets keep it simple: Armor destroyed.
                            Console.WriteLine($"{Name}'s MDC Armor has been DESTROYED!");
                            BodyArmor = null; 
                        }
                    }
                    else
                    {
                        // Direct to main body MDC
                        MDC -= damage;
                    }
                }
                else
                {
                    // MDC damage to SDC being...
                    // 1 MDC = 100 SDC roughly, or instant death.
                    // "Simplistic but accurate" -> MDC weapons vaporize SDC beings unless they have force fields (MDC armor).
                    Console.WriteLine($"{Name} takes MDC damage directly to SDC body! VAPORIZED!");
                    HitPoints = -9999;
                    SDC = 0;
                }
            }
            else // SDC Damage
            {
                if (IsMDCCreature || (BodyArmor != null && BodyArmor.IsMDC))
                {
                    // SDC damage bounces off MDC armor/hide
                    Console.WriteLine($"{Name} ignores SDC damage (MDC armor/body).");
                    return;
                }

                // Check AR
                int currentAR = BodyArmor != null ? BodyArmor.AR : AR;
                // Note: AR check usually happens on the attack roll (To Hit > AR). 
                // But if we are here, we assume the attack "Hit". 
                // However, PALLADIUM rules say: Attack Roll > AR = Damage to HP/SDC. 
                // Attack Roll < AR but > 4 (or 8/10/12) = Damage to Armor.
                // We will handle AR logic in the Resolver, passing final damage here meant it penetrated or damaged armor.
                
                // Example logic for Taking Damage assumed to be aiming at Body:
                if (BodyArmor != null && BodyArmor.SDC > 0)
                {
                    BodyArmor.SDC -= damage;
                    if (BodyArmor.SDC < 0) 
                    {
                         BodyArmor = null; 
                         Console.WriteLine($"{Name}'s SDC Armor Destroyed!");
                    }
                }
                else
                {
                    if (SDC > 0)
                    {
                        SDC -= damage;
                        if (SDC < 0)
                        {
                            HitPoints += SDC; // SDC is negative, so this subtracts
                            SDC = 0;
                        }
                    }
                    else
                    {
                        HitPoints -= damage;
                    }
                }
            }
        }
    }
}
