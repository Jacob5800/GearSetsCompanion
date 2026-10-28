# Gear Sets Companion (Beta)

Separate Dalamud companion for Jacobâ€™s Gear Sets. No network service or item movement.

Build: `dotnet build GearSetsCompanion.csproj -c Release`. Add the resulting DLL to Dalamudâ€™s development plugin locations, load it, and use `/gearsets` to choose the existing JacobGearPlanner directory.

Armoire ownership is exported only after Cabinet reports Loaded. Minion imports the complete matching-character snapshot and retains it between visits. Character name and home-world ID must match.

Enable **Highlight planned items in containers** in Minion Plan and **Enable planned-item highlights** in this plugin. Both preferences persist. Highlighting defaults off in Minion. Selected gear and its accepted alternatives are included.

Implemented overlays: normal and fully expanded inventory, normal/premium saddlebag. Other inventory layouts, retainer, armoury, dresser and armoire slot overlays still require mappings and runtime verification. Armoire ownership is independent of slot highlighting.

Bridge: schemaVersion 1; plan.json contains character, highlight, itemIds; armoire.json contains character, complete, scannedAt, itemIds. Files stay local. Armoire writes use atomic replacement; transient invalid plan reads clear highlights until the next one-second check. No native UI colours are changed.

Validation: Release build passed; Minion syntax, persistence and render tests passed. Native armoire ownership and overlay alignment are pending in-game checks. This beta has not completed in-game verification.

## Install

Add `https://raw.githubusercontent.com/Jacob5800/GearSetsCompanion/main/repo.json` in Dalamud Settings → Experimental → Custom Plugin Repositories, then install Gear Sets Companion (Beta). Use `/gearsets` to configure the existing planner folder.

This standalone repository is the canonical source for the companion.
