# System Features & Mechanics

This document serves as a comprehensive context dump of the functionality implemented in the Palladium Rifts Combat Simulator.

## Core Combat Engine
- **Melee Round System**: Combat is divided into 15-second Melee Rounds.
- **Initiative**: Rolled once per round (D20 + Bonus). Winner decides order (currently strictly sequential high-to-low).
- **Action Point Economy**:
  - Combatants have a set number of **Attacks Per Melee**.
  - **Attacking** costs 1 Action.
  - **Dodging** costs 1 Action.
  - Actions are tracked dynamically; a combatant cannot Dodge if they have 0 Actions left.

## Damage & Health Models
- **M.D.C. (Mega-Damage Capacity)**:
  - Supported for structures, armor, and creatures.
  - **Invulnerability**: M.D.C. armor/skin ignores all S.D.C. damage.
  - **Bleed-through**: When M.D.C. armor is destroyed, remaining damage and subsequent attacks apply to the main body.
- **S.D.C. & Hit Points**:
  - Supported for non-M.D.C. beings.
  - AR (Armor Rating) logic implemented for S.D.C. armor (attacks below AR bounce, above AR penetrate).
- **Vaporization**: M.D.C. attacks vs S.D.C. targets result in instant destruction.

## Weapon Systems
- **Dice Logic**: Supports standard dice (nD6) and Multipliers (e.g., `3D6 x 10`).
- **Ammunition & Payload**:
  - Weapons track `CurrentAmmo` and `MaxPayload`.
  - Ammo is consumed on *attempt* (misses still waste ammo).
  - **Auto-Switching**: AI automatically swaps to the next available weapon when current ammo is depleted.
- **Volleys**:
  - Supports variable volley sizes (e.g., firing 2, 4, or more missiles at once).
  - Damage is calculated as Sum of Dice (roughly) or Multiplied, tracked via `VolleySize`.

## Tactical AI & Logic
The simulator includes "smart" behaviors to mock real player decisions:

### 1. Intelligent Dodge
Defenders do not blindly Dodge every attack (wasting their Actions).
- **Probability Check**: The AI calculates the roll needed to beat the attacker's Strike.
- **Decision**: 
  - If the chance to Dodge is **Reasonable (>50%)**, they will dodge.
  - If the chance is low (requires a natural 18-20), they save their action for an attack, unless the incoming attack is critical.
  - **No Actions**: If Actions Left is 0, Dodge is impossible.

### 2. Dynamic Firepower (The "Finisher" Logic)
Missile Launchers utilize variable volley sizes (`MaxVolleySize`):
- **Baiting**: If the Target has Actions remaining, the attacker fires conservative volleys (e.g., 2 missiles) to force the target to Dodge and burn actions.
- **Unleash Hell**: If the Target has **0 Actions remaining**, the attacker scales up to `MaxVolleySize` (e.g., 4 missiles) to maximize damage on the defenseless target.

### 3. Melee Fallback
If all ranged ammunition is exhausted:
- Combatants resort to **"Hand to Hand (Punch)"**.
- Damage is scaled to their strength/nature (e.g., **Mega-Damage** punches for Robot Vehicles, avoiding infinite S.D.C. pinging loops).

## Current Scenario Loadout
The `Program.cs` currently simulates a duel between:
- **Glitter Boy**:
  - *Boom Gun*: 3D6x10 M.D. (Unlimited Ammo).
  - High Armor (770 M.D.C.).
- **UAR-1 Enforcer**:
  - *CR-6 Launcher* (Medium Range): 4D6x10 M.D. (Max Volley 4).
  - *CR-10 Launcher* (Short Range): 2D6x10 M.D. (Max Volley 4, Payload 10).
  - *CR-20 Launcher* (Mini): 4D4x10 M.D. (Max Volley 4).
  - *Rail Gun*: 1D6x10 M.D. (Burst).
