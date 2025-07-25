# FishEggs Development Scratch Pad

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
