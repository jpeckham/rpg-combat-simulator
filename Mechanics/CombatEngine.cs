using System;
using System.Collections.Generic;
using System.Linq;
using PalladiumRiftsCombatSim.Models;

namespace PalladiumRiftsCombatSim.Mechanics
{
    public class CombatEngine
    {
        private List<Combatant> _sideA;
        private List<Combatant> _sideB;
        private int _roundCount = 0;

        public CombatEngine(List<Combatant> sideA, List<Combatant> sideB)
        {
            _sideA = sideA;
            _sideB = sideB;
        }

        public void RunEncounter()
        {
            Console.WriteLine("=== ENCOUNTER START ===");
            
            while (IsSideAlive(_sideA) && IsSideAlive(_sideB))
            {
                _roundCount++;
                Console.WriteLine($"\n--- MELEE ROUND {_roundCount} ---");
                
                // Status Report
                foreach(var c in _sideA.Concat(_sideB))
                {
                    if (c.BodyArmor != null)
                        Console.WriteLine($"  [{c.Name}] Armor MDC: {c.BodyArmor.MDC}");
                    else if (c.MDC > 0)
                        Console.WriteLine($"  [{c.Name}] MDC: {c.MDC}");
                    else
                         Console.WriteLine($"  [{c.Name}] SDC: {c.SDC} HP: {c.HitPoints}");
                }
                
                // 1. Initiative
                var initiativeOrder = RollInitiative();
                
                // 2. Execute Actions (Cycle through Attacks Per Melee)
                ExecuteRound(initiativeOrder);
            }

            Console.WriteLine("\n=== ENCOUNTER END ===");
            if (IsSideAlive(_sideA)) Console.WriteLine("SIDE A WINS!");
            else Console.WriteLine("SIDE B WINS!");
        }

        private bool IsSideAlive(List<Combatant> side)
        {
            return side.Any(c => !c.IsDestroyed);
        }

        private List<Combatant> RollInitiative()
        {
            var order = new List<(Combatant c, int init)>();
            
            foreach(var c in _sideA.Concat(_sideB))
            {
                if (c.IsDestroyed) continue;
                int roll = Die.Roll(20);
                int total = roll + c.BonusInitiative;
                order.Add((c, total));
                Console.WriteLine($"{c.Name} rolls Initiative: {roll} + {c.BonusInitiative} = {total}");
            }
            
            return order.OrderByDescending(x => x.init).Select(x => x.c).ToList();
        }

        private void ExecuteRound(List<Combatant> executionOrder)
        {
            // Reset actions for the round
            foreach(var c in executionOrder)
            {
                c.ActionsLeft = c.AttacksPerMelee;
            }

            bool anyActionLeft = true;
            
            while (anyActionLeft)
            {
                anyActionLeft = false;
                
                foreach(var attacker in executionOrder)
                {
                    if (attacker.IsDestroyed) continue;
                    if (attacker.ActionsLeft > 0)
                    {
                        anyActionLeft = true;
                        
                        // Select Target (First living enemy)
                        Combatant target = FindTarget(attacker);
                        if (target == null) break; // Combat over?

                        // Configure Attack (Dynamic Volleys)
                        attacker.ConfigureAttack(target);

                        // Perform Action
                        CombatResolver.ResolveAttack(attacker, target);
                        
                        attacker.ActionsLeft--;
                         Console.WriteLine($"  > {attacker.Name} Actions Left: {attacker.ActionsLeft}");
                        
                        // Check if combat ended mid-round
                        if (!IsSideAlive(_sideA) || !IsSideAlive(_sideB)) return;
                    }
                }
            }
        }

        private Combatant FindTarget(Combatant attacker)
        {
            // If attacker is in Side A, target Side B, else Side A
            List<Combatant> enemies = _sideA.Contains(attacker) ? _sideB : _sideA;
            return enemies.FirstOrDefault(e => !e.IsDestroyed);
        }
    }
}
