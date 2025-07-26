using System.Linq;
using RimWorld;
using Verse;
using LudeonTK;

namespace FishEggs
{
    /// <summary>
    /// Debug commands for testing fish egg functionality
    /// </summary>
    public static class DebugCommands
    {
        [DebugAction("Fish Eggs", "List all fish eggs in category", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ListFishEggsInCategory()
        {
            var fishEggsCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail("FishEggs");
            if (fishEggsCategory == null)
            {
                Log.Error("[FishEggs] FishEggs category not found!");
                return;
            }

            var fishEggThings = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.thingCategories != null && def.thingCategories.Contains(fishEggsCategory))
                .ToList();

            Log.Message($"[FishEggs] Found {fishEggThings.Count} items in FishEggs category:");
            
            foreach (var thing in fishEggThings)
            {
                Log.Message($"  - {thing.defName}: {thing.label}");
            }
        }

        [DebugAction("Fish Eggs", "Spawn salmon egg", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnSalmonEgg()
        {
            var salmonEgg = DefDatabase<ThingDef>.GetNamedSilentFail("FishEgg_Fish_Salmon");
            if (salmonEgg == null)
            {
                Log.Error("[FishEggs] FishEgg_Fish_Salmon not found!");
                return;
            }

            var thing = ThingMaker.MakeThing(salmonEgg);
            GenPlace.TryPlaceThing(thing, UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);
            Log.Message($"[FishEggs] Spawned {salmonEgg.label}");
        }

        [DebugAction("Fish Eggs", "Spawn base fish egg", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnBaseFishEgg()
        {
            var baseEgg = DefDatabase<ThingDef>.GetNamedSilentFail("FishEgg_Base");
            if (baseEgg == null)
            {
                Log.Error("[FishEggs] FishEgg_Base not found!");
                return;
            }

            var thing = ThingMaker.MakeThing(baseEgg);
            GenPlace.TryPlaceThing(thing, UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);
            Log.Message($"[FishEggs] Spawned {baseEgg.label}");
        }

        [DebugAction("Fish Eggs", "List all ThingDefs with 'FishEgg' in name", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ListAllFishEggDefs()
        {
            var fishEggDefs = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.defName.Contains("FishEgg"))
                .OrderBy(def => def.defName)
                .ToList();

            Log.Message($"[FishEggs] Found {fishEggDefs.Count} ThingDefs with 'FishEgg' in name:");
            
            foreach (var def in fishEggDefs)
            {
                var categories = def.thingCategories != null ? string.Join(", ", def.thingCategories.Select(c => c.defName)) : "None";
                Log.Message($"  - {def.defName}: '{def.label}' (Categories: {categories})");
            }
        }

        [DebugAction("Fish Eggs", "Check FishEggs category details", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void CheckFishEggsCategoryDetails()
        {
            var fishEggsCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail("FishEggs");
            if (fishEggsCategory == null)
            {
                Log.Error("[FishEggs] FishEggs category not found!");
                return;
            }

            Log.Message($"[FishEggs] FishEggs category found:");
            Log.Message($"  - DefName: {fishEggsCategory.defName}");
            Log.Message($"  - Label: {fishEggsCategory.label}");
            Log.Message($"  - Parent: {fishEggsCategory.parent?.defName ?? "None"}");
            
            // List all child categories
            var childCategories = DefDatabase<ThingCategoryDef>.AllDefs
                .Where(cat => cat.parent == fishEggsCategory)
                .ToList();
            
            Log.Message($"  - Child categories: {childCategories.Count}");
            foreach (var child in childCategories)
            {
                Log.Message($"    - {child.defName}: {child.label}");
            }

            // List all things in this category
            var thingsInCategory = fishEggsCategory.childThingDefs;
            Log.Message($"  - Direct child things: {thingsInCategory?.Count ?? 0}");
            if (thingsInCategory != null)
            {
                foreach (var thing in thingsInCategory)
                {
                    Log.Message($"    - {thing.defName}: {thing.label}");
                }
            }

            // List all descendants (including from child categories)
            var allDescendants = fishEggsCategory.DescendantThingDefs.ToList();
            Log.Message($"  - All descendant things: {allDescendants.Count}");
            foreach (var thing in allDescendants.Take(10)) // Limit to first 10 to avoid spam
            {
                Log.Message($"    - {thing.defName}: {thing.label}");
            }
            if (allDescendants.Count > 10)
            {
                Log.Message($"    ... and {allDescendants.Count - 10} more");
            }
        }

        [DebugAction("Fish Eggs", "Spawn all fish egg types", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnAllFishEggTypes()
        {
            var fishEggDefs = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.defName.StartsWith("FishEgg_") && def.defName != "FishEgg_Base")
                .OrderBy(def => def.defName)
                .Take(5) // Limit to first 5 to avoid spam
                .ToList();

            if (fishEggDefs.Count == 0)
            {
                Log.Error("[FishEggs] No FishEgg_ definitions found!");
                return;
            }

            var spawnCell = UI.MouseCell();
            var map = Find.CurrentMap;
            
            Log.Message($"[FishEggs] Spawning {fishEggDefs.Count} fish egg types:");
            
            for (int i = 0; i < fishEggDefs.Count; i++)
            {
                var def = fishEggDefs[i];
                var thing = ThingMaker.MakeThing(def);
                var targetCell = spawnCell + new IntVec3(i, 0, 0); // Spawn in a line
                
                if (GenPlace.TryPlaceThing(thing, targetCell, map, ThingPlaceMode.Near))
                {
                    Log.Message($"  - Spawned {def.label} ({def.defName})");
                }
                else
                {
                    Log.Warning($"  - Failed to spawn {def.label}");
                }
            }
        }
    }
}
