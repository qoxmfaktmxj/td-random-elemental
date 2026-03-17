# Development Status

## Current Build State

This repository now contains a playable MVP combat slice for the 3D character-driven random elemental tower defense prototype.

Current implemented flow:

1. The project boots into `MVP_Arena`.
2. A runtime player is spawned and can move with `WASD`.
3. A board of fixed tower nodes is generated.
4. Waves spawn enemies automatically.
5. The player can summon a random tower on an empty node with `E`.
6. Towers attack enemies automatically.
7. Matching towers can be merged with `E`.
8. Placed towers can be sold with `Q`.
9. A runtime HUD shows gold, core HP, wave state, controls, and interaction prompts.
10. Fire, Water, and Earth towers apply distinct elemental combat effects.
11. The last wave is a boss encounter with win/lose flow and restart support.

## Completed Tasks

- `T01` Project bootstrap
- `T02` Static data layer
- `T03` Run state and economy
- `T04` Player movement and interaction
- `T05` Tower board system
- `T06` Enemy pathing and survival
- `T07` Wave spawning
- `T08` Tower attack loop
- `T09` Random summon and placement
- `T10` Merge and sell
- `T11` Element effects
- `T12` HUD and world UI
- `T14` Run flow and boss structure

## Remaining Tasks

### High Priority

- `T13` Pity / guided summon logic
  - Prevent early dead-roll frustration
  - Make summon outcomes feel fair without killing randomness

- `T15` Presentation pass
  - Summon flash
  - Merge burst
  - Tier-up visual escalation
  - Boss intro sequence

### Medium Priority

- Authored content migration
  - Replace runtime fallback towers, enemies, and waves with authored ScriptableObject assets
  - Add authored tier 2~5 tower definitions instead of runtime-generated merge upgrades

- Production UI conversion
  - Replace the current `OnGUI` HUD with Canvas/TMP UI
  - Add a proper result screen and boss warning panel

## Current Control Scheme

- `WASD`: move
- `Left Shift`: dash
- `E`: summon on empty node / merge on merge-ready tower node
- `Q`: sell tower on occupied node
- `R`: restart run

## Current Runtime Fallback Content

- Tower pool:
  - `Fire Attack T1`
  - `Water Control T1`
  - `Earth Impact T1`

- Waves:
  - 5 standard runtime fallback waves + 1 boss runtime wave

- Enemy visuals:
  - Primitive placeholder enemies with archetype-based shapes/colors

- Tower visuals:
  - Primitive placeholder towers with tier-based scale growth

## Known Technical Limits

- The game is still driven by runtime-generated fallback content.
- Merge upgrade definitions are generated at runtime, not yet backed by authored tier assets.
- The HUD is currently an `OnGUI` debug-style runtime UI, not a final production UI.
- Element effects are implemented, but VFX and authored audiovisual feedback are still placeholder-level.
- Run end and boss flow are implemented, but they still rely on runtime banners and fallback content.
- Boss gimmicks currently disable empty edge nodes only; there are no boss attacks, summons, or cinematics yet.

## Recommended Next Order

1. Add pity logic so randomness becomes fairer.
2. Replace runtime fallback data with authored tower/enemy/wave assets.
3. Replace the debug HUD with production UI.
4. Add presentation polish after the systems are stable.
