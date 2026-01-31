using System;
using System.Collections.Generic;
using PalladiumRiftsCombatSim.Models;
using PalladiumRiftsCombatSim.Mechanics;

namespace PalladiumRiftsCombatSim
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Palladium Rifts Combat Simulator v0.1");

            // Setup Scenario: Glitter Boy vs UAR-1 Enforcer

            // 1. Glitter Boy
            var glitterBoy = new Combatant("Glitter Boy", isMdcCreature: false) 
            {
                HitPoints = 50,
                SDC = 60,
                AttacksPerMelee = 4, 
                BonusStrike = 6,
                BonusParry = 8,
                BonusDodge = 8,
                BonusInitiative = 4
            };
            // GB Armor (USA-G10)
            glitterBoy.BodyArmor = new Armor("Glitter Boy Power Armor", mdc: 770, sdc: 0, ar: 0, isMdc: true);
            
            // GLITTER BOY WEAPON
            var boomGun = new Weapon()
            {
                Name = "Boom Gun (Flechette)",
                IsMDC = true,
                DamageDiceCount = 3, 
                DamageDiceSides = 6,
                Multiplier = 10,
                DamageBonus = 0,
                MaxPayload = 1000, // Effectively unlimited for this fight
                CurrentAmmo = 1000,
                VolleySize = 1
            };
            glitterBoy.Weapons.Add(boomGun);

            // 2. UAR-1 Enforcer
            var uar1 = new Combatant("UAR-1 Enforcer", isMdcCreature: false)
            {
                HitPoints = 40,
                SDC = 50,
                AttacksPerMelee = 5,
                BonusStrike = 4,
                BonusParry = 6,
                BonusDodge = 6,
                BonusInitiative = 3
            };
            uar1.BodyArmor = new Armor("UAR-1 Enforcer Robot", mdc: 350, sdc: 0, ar: 0, isMdc: true);
            
            // UAR-1 WEAPONS (Rifts Ultimate Edition / Sourcebook 1)
            
            // 1. CR-6 Medium-Range Missile Launcher (Shoulders)
            // Payload: 8 (4 per shoulder). 
            // Damage: Medium HE (2D6x10). Volley: Let's fire 2 at a time.
            var cr6 = new Weapon()
            {
                Name = "CR-6 Medium-Range Missile Launcher",
                IsMDC = true,
                DamageDiceCount = 2, 
                DamageDiceSides = 6,
                Multiplier = 10,
                VolleySize = 2,
                MaxVolleySize = 4, // Variable 1-4
                MaxPayload = 6,
                CurrentAmmo = 6
            };
            uar1.Weapons.Add(cr6);

            // 2. CR-10 Short-Range Missile Launcher (Chest)
            var cr10 = new Weapon()
            {
                Name = "CR-10 Short-Range Missile Launcher",
                IsMDC = true,
                DamageDiceCount = 1,
                DamageDiceSides = 6,
                Multiplier = 10,
                VolleySize = 2,
                MaxVolleySize = 4, // Variable 1-4
                MaxPayload = 10,
                CurrentAmmo = 10
            };
            uar1.Weapons.Add(cr10);

            // 3. CR-20 Mini-Missile Launcher (Turret)
            var cr20 = new Weapon()
            {
                Name = "CR-20 Mini-Missile Launcher",
                IsMDC = true,
                DamageDiceCount = 1,
                DamageDiceSides = 4,
                Multiplier = 10,
                VolleySize = 4,
                MaxVolleySize = 4,
                MaxPayload = 20,
                CurrentAmmo = 20
            };
            uar1.Weapons.Add(cr20);

            // 4. C-50 Rail Gun
            // Damage: 1D6x10 (Previously 1D4x10 in earlier books, 1D6x10 in UE?) 
            // Let's stick to 1D6x10.
            var c50 = new Weapon()
            {
                Name = "C-50 Enforcer Rail Gun (Burst)",
                IsMDC = true,
                DamageDiceCount = 1,
                DamageDiceSides = 6,
                Multiplier = 10,
                VolleySize = 1, // 1 Burst
                MaxPayload = 100, // Drums
                CurrentAmmo = 100
            };
            uar1.Weapons.Add(c50);

            // FALLBACK MELEE WEAPONS (Ensures combat continues with MDC damage if ammo runs dry)
            
            // Glitter Boy Punch/Kick
            glitterBoy.Weapons.Add(new Weapon()
            {
                Name = "Hand to Hand (Punch)",
                IsMDC = true,
                DamageDiceCount = 1,
                DamageDiceSides = 6, // 1D6 MD for simplicity
                Multiplier = 1,
                DamageBonus = 0,
                MaxPayload = 10000,
                CurrentAmmo = 10000, // Infinite
                VolleySize = 1
            });

            // UAR-1 Enforcer Punch
            uar1.Weapons.Add(new Weapon()
            {
                Name = "Hand to Hand (Punch)",
                IsMDC = true,
                DamageDiceCount = 2, // 2D6 Power Punch equivalent for faster resolution
                DamageDiceSides = 6,
                Multiplier = 1,
                DamageBonus = 0,
                MaxPayload = 10000,
                CurrentAmmo = 10000, // Infinite
                VolleySize = 1
            });

            var sideA = new List<Combatant> { glitterBoy };
            var sideB = new List<Combatant> { uar1 };

            var engine = new CombatEngine(sideA, sideB);
            engine.RunEncounter();
            
            Console.WriteLine("Simulation Complete.");
        }
    }
}
