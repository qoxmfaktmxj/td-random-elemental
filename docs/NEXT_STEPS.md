# Next Steps

## Immediate

- `T13` Guided summon / pity
  - Make repeated low-roll openings less punishing.
  - Surface pity state in the HUD instead of leaving it as a hidden numeric meter.

- Authored content replacement
  - Replace runtime fallback tower definitions with real `ScriptableObject` assets.
  - Replace runtime fallback enemy definitions and fallback waves with authored assets.

## Short Term

- Production HUD
  - Move from `OnGUI` to Canvas + TextMeshPro.
  - Add a result overlay for win/lose.
  - Add a dedicated boss warning panel and boss HP display.

- Presentation pass
  - Summon flash
  - Merge burst
  - Tier-up silhouette growth
  - Boss spawn intro

## Medium Term

- Tower content expansion
  - Author tier 2~5 definitions for Fire / Water / Earth.
  - Add stronger visual separation between role cores and element shells.

- Boss design expansion
  - Replace the current node-disable gimmick with 2~3 authored boss mechanics.
  - Add boss-specific VFX, sound, and telegraphs.

## Technical Debt

- Add play mode tests for:
  - Wave clear / run complete
  - Core death / run fail
  - Merge upgrade flow
  - Boss node lockdown / release

- Reduce runtime-generated placeholder objects:
  - Replace primitive enemies
  - Replace primitive towers
  - Replace fallback-only balance data
