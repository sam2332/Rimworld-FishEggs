# FishEggs Development Scratch Pad

## Latest Fixes Applied:

### 8. Vanilla Trader DefName Updates for RimWorld 1.6 ✅ FIXED
- **Problem:** Vanilla trader patches failing because trader defNames changed in RimWorld 1.6
  - `BulkGoods` → `Orbital_BulkGoods` and `Caravan_Outlander_BulkGoods`
  - `ExoticGoods` → `Orbital_Exotic`
  - `Orbital_ExoticGoodsTrader` → doesn't exist anymore
- **Root Cause:** Hardcoded old trader defNames from earlier RimWorld versions
- **Solution:** Updated trader patch XPaths to match current RimWorld 1.6 trader definitions:
  ```xml
  <xpath>/Defs/TraderKindDef[defName="Orbital_BulkGoods"]/stockGenerators</xpath>
  <xpath>/Defs/TraderKindDef[defName="Caravan_Outlander_BulkGoods"]/stockGenerators</xpath>
  <xpath>/Defs/TraderKindDef[defName="Orbital_Exotic"]/stockGenerators</xpath>
  ```
- **Files:** `Patches/FishEggs_Vanilla_Traders.xml`
- **Key Insight:** Always verify trader defNames against current game XML files when updating for new RimWorld versions

### 7. PatchOperationFindMod XML Structure Error ✅ FIXED
- **Problem:** `Exception parsing <match>true</match> to type Verse.PatchOperation` and `XML error: doesn't correspond to any field in type Success`
- **Root Cause:** Incorrect XML structure in `FishEggs_VET_Traders.xml`
  1. Used `<match>true</match>` instead of `<match Class="PatchOperationSequence">`
  2. Used `<success>` element (doesn't exist in PatchOperationFindMod)
  3. Put multiple `<li>` elements directly in wrong parent
- **Solution:** Fixed XML structure following RimWorld's pattern:
  ```xml
  <Operation Class="PatchOperationFindMod">
    <mods><li>VanillaExpanded.VanillaTraders</li></mods>
    <match Class="PatchOperationSequence">
      <operations>
        <li Class="PatchOperationAdd">...</li>
        <li Class="PatchOperationAdd">...</li>
      </operations>
    </match>
  </Operation>
  ```
- **Files:** `Patches/FishEggs_VET_Traders.xml`
- **Key Insight:** PatchOperationFindMod expects `match` and `nomatch` to be single PatchOperation objects, not lists or boolean values

## Fish Discovery Summary

### Vanilla + Odyssey Fish Types (Found via Game XML):
**Freshwater Fish:**
- Fish_Bass (common in temperate/grasslands)
- Fish_Salmon (common in cold/temperate - can live in both fresh/salt)
- Fish_Tilapia (common in warm/tropical)
- Fish_Catfish (uncommon in most temperate/warm, large size)
- Fish_Piranha (uncommon in tropical, aggressive)
- Fish_Frostfish (uncommon in cold, bioengineered, large)
- Fish_Guppy (small, low nutrition, high market value)

**Saltwater Fish:**
- Fish_Salmon (common in cold/temperate)
- Fish_Cod (common in cold)
- Fish_Bluefish (common in temperate/warm)
- Fish_Dogfish (uncommon cold, small shark, large)
- Fish_Marlin (uncommon temperate/warm, large)
- Fish_Tuna (uncommon warm/tropical, large)
- Fish_Flounder (uncommon temperate/warm, large)

**Special/Biotech:**
- Fish_Toxfish (polluted fish, requires Biotech)

### VFE Additional Fish (from InternalDefOf):
- VCEF_Crayfish (requires VMemesE)
- VCEF_ButterFish (requires VMemesE)
- VCEF_FreshwaterStingray (requires VMemesE)
- VCEF_FlyingFish (requires VMemesE)
- VCEF_Arapaima (requires VMemesE)
- VCEF_ShortfinMakoShark (requires VMemesE)

## Water Type System Discovery:

### Game Source WaterBody.cs shows:
- WaterBodyType.Freshwater vs WaterBodyType.Saltwater
- BiomeDef.fishTypes has separate lists:
  - freshwater_Common
  - freshwater_Uncommon  
  - saltwater_Common
  - saltwater_Uncommon

### Integration Points:

1. **VFE Integration:** VFE patches FishingUtility.GetCatchesFor via Harmony
2. **Water Body Detection:** Use WaterBody.waterBodyType to determine fresh vs salt
3. **Fish Discovery:** Use reflection to find all ThingDefs with thingCategories containing "Fish"
4. **Water Type Assignment:** Need to determine which fish go in fresh vs salt water based on biome definitions

## Implementation Plan:

### Phase 1: Core Infrastructure
1. ✅ Update mod metadata 
2. Create base classes:
   - FishEggDef (custom def type)
   - ThingDef_FishEgg (thing class)
   - CompUseEffect_SeedWater (use effect component)
   - MapComponent_StockedTiles (tracks seeded tiles)

### Phase 2: Fish Egg Generation
1. Use reflection to discover all fish ThingDefs
2. Dynamically create fish egg ThingDefs for each fish
3. Determine water type compatibility using biome analysis

### Phase 3: Seeding Mechanics
1. Right-click float menu
2. Targeting cursor system
3. Water type validation
4. Integration with VFE/Odyssey fishing system

### Phase 4: Trading & Drops
1. Trader integration
2. Fishing drop patches (hook into VFE system)

### Phase 5: Settings & Polish
1. Mod settings UI
2. Animals XP rewards
3. Visual effects (ripples)

## Key Game Classes to Study:
- FishingUtility (core fishing logic)
- WaterBody (water type detection)
- JobDriver_Fish (fishing job)
- BiomeDef (fish type definitions)
- ThingWithComps (base for fish eggs)
- CompUseEffect (use effect base class)

## Critical Bug Fixes Applied:

### 1. XML IntRange Format Issue
- **Problem:** TraderKinds_FishEggs.xml used hyphens (`3-8`) in IntRange values
- **Solution:** Changed to tildes (`3~8`) - RimWorld's XML parser requires tildes for ranges
- **File:** Defs/TraderKindDefs/TraderKinds_FishEggs.xml

### 2. Job Count Issue  
- **Problem:** SeedWaterSource job had count = -1, causing hauling error in Toils_Haul.StartCarryThing
- **Root Cause:** JobMaker.MakeJob() without specifying count defaults to 0/-1
- **Solution:** Set job.count = 1 when creating SeedWaterSource jobs  
- **File:** Source/ThingWithComps_FishEgg.cs
- **Key Insight:** All RimWorld jobs that handle items MUST have a valid positive count for hauling to work

## Hash Collision Fix Applied:

### 3. Short Hash Collision Issue
- **Problem:** FishEggDefGenerator was manually generating short hashes using simple hash function, causing collisions with existing game definitions (Hash collision warnings in console)
- **Root Cause:** Manual short hash generation `(defName + type).GetHashCode() & 0xFFFF` doesn't check for existing hashes
- **Solution:**
  1. Removed manual `GiveShortHash()` method
  2. Let RimWorld's DefDatabase automatically assign unique short hashes
  3. Made defNames more unique by including original fish's shortHash: `FishEgg_{fishDefName}_{fishShortHash}`
  4. Added delayed generation using `LongEventHandler.QueueLongEvent()` to ensure all mods load first
  5. Added safety check for existing definitions
- **Files:** Source/FishEggDefGenerator.cs
- **Key Insight:** Never manually assign short hashes - let RimWorld handle collision detection automatically

## Updated Hash Collision Fix Applied:

### 4. Comprehensive Short Hash Collision Resolution 
- **Problem:** Still getting hash collisions like `FishEgg_VCEF_RawAngelfish_4030 and FishEgg_Fish_Salmon_46640: both have short hash 0`
- **Root Cause:** RimWorld's automatic hash assignment during late loading wasn't working properly
- **New Solution:**
  1. **Explicit Hash Collection**: Collect all existing short hashes before generation
  2. **Collision-Free Generation**: Generate unique hashes by incrementing until finding free hash
  3. **Hash Reservation**: Track used hashes to prevent future collisions
  4. **Backward Compatibility**: Simplified defName format back to `FishEgg_{fishDefName}` for save compatibility
  5. **Better Logging**: Added collision resolution attempt logging
- **Files:** Source/FishEggDefGenerator.cs
- **Key Insight:** When dynamically generating defs, manually manage short hashes with collision detection

### 5. Missing VFE Fish Definition Issue
- **Problem:** `Could not load reference to Verse.ThingDef named FishEgg_VCEF_RawHalibut`
- **Root Cause:** VanillaTradingExpanded save data references fish egg that may not be generated
- **Investigation:** `VCEF_RawHalibut` exists in VFE (confirmed in XML files)
- **Likely Issue:** Fish discovery or generation timing problem
- **Files Checked:** VFE 1.6Odyssey definitions confirmed halibut exists

### 6. VanillaTradingExpanded FishEgg_Base Reference Issue ✅ SOLVED
- **Problem:** `Could not load reference to Verse.ThingDef named FishEgg_Base` errors during game initialization
- **Root Cause Investigation:** 
  1. **Initial Hypothesis**: Hardcoded reference in VTE mod ❌
  2. **Correct Cause**: VTE's `TradingManager` GameComponent saves price history data to world save files
  3. **Storage Location**: `priceHistoryRecorders` dictionary in save files contains `FishEgg_Base` key from old mod versions
  4. **Persistence**: VTE loads this data during initialization, before any save game is loaded
- **Evidence Found**: Multiple save files contain VTE price history with `FishEgg_Base` references:
  ```xml
  <li>FishEgg_Base</li>  <!-- in keys section -->
  <thingDef>FishEgg_Base</thingDef>  <!-- in values section -->
  ```
- **Solution Applied**: Fixed existing `Things_FishEgg_Base.xml` definition that had XML syntax errors
- **Additional Issues Found & Fixed**: 
  1. **XML Definition Syntax Errors** in `Things_FishEgg_Base.xml`:
     - `<category>ThingCategory.Item</category>` → `<category>Item</category>` ✅
     - `<altitudeLayer>AltitudeLayer.Item</altitudeLayer>` → `<altitudeLayer>Item</altitudeLayer>` ✅
     - `<drawerType>DrawerType.MapMeshOnly</drawerType>` → `<drawerType>MapMeshOnly</drawerType>` ✅
     - Missing `<tickerType>Rare</tickerType>` for CompRottable ✅
  2. **Duplicate Definition Error**: `Mod FishEggs.ProgrammerLily.com has multiple Verse.BuildableDefs named FishEgg_Base`
     - **Cause**: Both `Things_FishEgg_Base.xml` and `FishEgg_Base_Legacy.xml` contained `defName>FishEgg_Base</defName>`
     - **Fix**: Disabled `FishEgg_Base_Legacy.xml` by removing all definitions ✅
- **Files:** 
  - Investigation: `find_vte_cache.py` script
  - Fix: `Defs/ThingDefs/Things_FishEgg_Base.xml`
  - Disabled: `Defs/ThingDefs/FishEgg_Base_Legacy.xml`
- **Key Insights**: 
  1. RimWorld XML definitions don't use fully qualified enum names - use simple values like `Item`, not `ThingCategory.Item`
  2. RimWorld requires unique `defName` values across all definitions - duplicates are skipped and cause errors
