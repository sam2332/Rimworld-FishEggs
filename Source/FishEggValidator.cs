using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FishEggs
{
    /// <summary>
    /// Validates that fish eggs exist for all fish types and warns about missing ones
    /// </summary>
    public static class FishEggValidator
    {
        public static void ValidateFishEggs()
        {
            var allFish = FishEggUtility.GetAllFishDefs().ToList();
            var missingEggs = new List<string>();
            var foundEggs = 0;
            
            Log.Message($"[FishEggs] Starting validation of fish eggs for {allFish.Count} fish types...");
            
            foreach (var fishDef in allFish)
            {
                // Check if there's a corresponding fish egg
                var expectedEggDefName = $"FishEgg_{fishDef.defName}";
                var eggDef = DefDatabase<ThingDef>.GetNamedSilentFail(expectedEggDefName);
                
                if (eggDef != null)
                {
                    foundEggs++;
                }
                else
                {
                    missingEggs.Add(fishDef.defName);
                    Log.Warning($"[FishEggs] ✗ Missing egg definition for fish: {fishDef.defName} (expected: {expectedEggDefName})");
                }
            }
            
            // Report summary
            Log.Message($"[FishEggs] Validation complete: {foundEggs}/{allFish.Count} fish have corresponding eggs");
            
            if (missingEggs.Any())
            {
                Log.Warning($"[FishEggs] Missing fish eggs for {missingEggs.Count} fish types:");
                foreach (var missingFish in missingEggs)
                {
                    Log.Warning($"[FishEggs] - {missingFish}");
                }
                
                Log.Warning("[FishEggs] Consider adding ThingDef entries for these missing fish eggs!");
            }
            else
            {
                Log.Message("[FishEggs] ✓ All fish types have corresponding egg definitions!");
            }
            
            // Validate category assignments
            ValidateFishEggCategories();
        }
        
        private static void ValidateFishEggCategories()
        {
            var fishEggsCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail("FishEggs");
            if (fishEggsCategory == null)
            {
                Log.Error("[FishEggs] FishEggs category not found! Fish eggs won't appear in stockpiles properly.");
                return;
            }
            
            var eggDefsInCategory = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.thingCategories?.Contains(fishEggsCategory) == true)
                .ToList();
                
         
        }
    }
}
