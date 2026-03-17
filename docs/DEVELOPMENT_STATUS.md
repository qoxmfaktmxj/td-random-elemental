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

## Remaining Tasks

### High Priority

- `T12` HUD and world UI
  - Gold, core HP, wave state, remaining enemies, interaction prompt, control hints
  - This is the most important next step because the current build is functional but hard to read without editor inspection

- `T11` Element effects
  - Fire burn, Water slow, Earth impact behavior
  - Right now towers differ mainly by stats and targeting, not by visible combat effect

- `T14` Run flow and boss structure
  - Clear win/lose handling
  - Boss-specific encounter structure
  - Node disable gimmick or other boss interaction

### Medium Priority

- `T13` Pity / guided summon logic
  - Prevent early dead-roll frustration
  - Make summon outcomes feel fair without killing randomness

- `T15` Presentation pass
  - Summon flash
  - Merge burst
  - Tier-up visual escalation
  - Boss intro sequence

## Current Control Scheme

- `WASD`: move
- `Left Shift`: dash
- `E`: summon on empty node / merge on merge-ready tower node
- `Q`: sell tower on occupied node

## Current Runtime Fallback Content

- Tower pool:
  - `Fire Attack T1`
  - `Water Control T1`
  - `Earth Impact T1`

- Waves:
  - 6 runtime fallback waves

- Enemy visuals:
  - Primitive placeholder enemies

- Tower visuals:
  - Primitive placeholder towers with tier-based scale growth

## Known Technical Limits

- The game is still driven by runtime-generated fallback content.
- There is no HUD yet, so gameplay readability depends on Unity inspector or scene observation.
- Merge upgrade definitions are generated at runtime, not yet backed by authored tier assets.
- Element effects are not yet applied to enemies; only base damage and targeting behavior are active.
- There is no explicit run-end screen or boss encounter flow yet.

## Recommended Next Order

1. Build HUD/UI so the current game is readable without editor knowledge.
2. Add element effects so tower identities become visible in play.
3. Add run flow and boss structure so the loop has a real end state.
4. Add pity logic so randomness becomes fairer.
5. Add presentation polish after the systems are stable.
