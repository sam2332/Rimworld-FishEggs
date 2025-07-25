using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace FishEggs
{
    /// <summary>
    /// Utility class for managing fish and fish eggs
    /// </summary>
    public static class FishEggUtility
    {
        private static List<ThingDef> cachedFishDefs;
        private static Dictionary<ThingDef, WaterType> fishWaterTypes;
        
        /// <summary>
        /// Get all fish ThingDefs in the game using reflection
        /// </summary>
        public static List<ThingDef> GetAllFishDefs()
        {
            if (cachedFishDefs == null)
            {
                cachedFishDefs = new List<ThingDef>();
                
                // Find all ThingDefs that have "Fish" in their thingCategories
                foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
                {
                    if (thingDef.thingCategories?.Contains(ThingCategoryDefOf.Fish) == true)
                    {
                        cachedFishDefs.Add(thingDef);
                    }
                }
                
                Log.Message($"[FishEggs] Found {cachedFishDefs.Count} fish types: {string.Join(", ", cachedFishDefs.Select(f => f.defName))}");
            }
            
            return cachedFishDefs;
        }
        
        /// <summary>
        /// Determine the water type for a fish based on biome definitions
        /// </summary>
        public static WaterType GetFishWaterType(ThingDef fishDef)
        {
            if (fishWaterTypes == null)
            {
                AnalyzeFishWaterTypes();
            }
            
            return fishWaterTypes.TryGetValue(fishDef, out WaterType waterType) ? waterType : FishEggsMod.Settings.defaultWaterTypeForUntagged;
        }
        
        /// <summary>
        /// Analyze all biome definitions to determine fish water type preferences
        /// </summary>
        private static void AnalyzeFishWaterTypes()
        {
            fishWaterTypes = new Dictionary<ThingDef, WaterType>();
            
            foreach (var biome in DefDatabase<BiomeDef>.AllDefs)
            {
                if (biome.fishTypes == null) continue;
                
                // Check freshwater fish
                if (biome.fishTypes.freshwater_Common != null)
                {
                    foreach (var fishChance in biome.fishTypes.freshwater_Common)
                    {
                        if (!fishWaterTypes.ContainsKey(fishChance.fishDef))
                        {
                            fishWaterTypes[fishChance.fishDef] = WaterType.FreshWater;
                        }
                    }
                }
                
                if (biome.fishTypes.freshwater_Uncommon != null)
                {
                    foreach (var fishChance in biome.fishTypes.freshwater_Uncommon)
                    {
                        if (!fishWaterTypes.ContainsKey(fishChance.fishDef))
                        {
                            fishWaterTypes[fishChance.fishDef] = WaterType.FreshWater;
                        }
                    }
                }
                
                // Check saltwater fish
                if (biome.fishTypes.saltwater_Common != null)
                {
                    foreach (var fishChance in biome.fishTypes.saltwater_Common)
                    {
                        // Only set to saltwater if not already marked as freshwater (some fish like salmon are both)
                        if (!fishWaterTypes.ContainsKey(fishChance.fishDef))
                        {
                            fishWaterTypes[fishChance.fishDef] = WaterType.SaltWater;
                        }
                    }
                }
                
                if (biome.fishTypes.saltwater_Uncommon != null)
                {
                    foreach (var fishChance in biome.fishTypes.saltwater_Uncommon)
                    {
                        if (!fishWaterTypes.ContainsKey(fishChance.fishDef))
                        {
                            fishWaterTypes[fishChance.fishDef] = WaterType.SaltWater;
                        }
                    }
                }
            }
            
            Log.Message($"[FishEggs] Analyzed water types for {fishWaterTypes.Count} fish types");
        }
        
        /// <summary>
        /// Get the water type of a terrain
        /// </summary>
        public static WaterType GetTerrainWaterType(TerrainDef terrain)
        {
            // Check terrain tags first
            if (terrain.tags?.Contains("FreshWater") == true)
            {
                return WaterType.FreshWater;
            }
            
            if (terrain.tags?.Contains("SaltWater") == true)
            {
                return WaterType.SaltWater;
            }
            
            // Use reflection to check water body type if available
            try
            {
                var map = Find.CurrentMap;
                if (map?.waterBodyTracker != null)
                {
                    // Try to find a water body at any cell with this terrain
                    foreach (var cell in map.AllCells.Where(c => c.GetTerrain(map) == terrain).Take(10))
                    {
                        var waterBody = map.waterBodyTracker.WaterBodyAt(cell);
                        if (waterBody != null)
                        {
                            // Use reflection to get waterBodyType
                            var waterBodyTypeField = typeof(WaterBody).GetField("waterBodyType", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (waterBodyTypeField != null)
                            {
                                var waterBodyType = waterBodyTypeField.GetValue(waterBody);
                                if (waterBodyType.ToString() == "Freshwater")
                                {
                                    return WaterType.FreshWater;
                                }
                                else if (waterBodyType.ToString() == "Saltwater")
                                {
                                    return WaterType.SaltWater;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[FishEggs] Error determining water type for terrain {terrain.defName}: {ex.Message}");
            }
            
            // Fall back to default
            return FishEggsMod.Settings.defaultWaterTypeForUntagged;
        }
        
        /// <summary>
        /// Check if a cell is valid for seeding with a specific fish
        /// </summary>
        public static bool IsValidSeedingCell(IntVec3 cell, Map map, ThingDef fishDef, out string reason)
        {
            reason = "";
            
            // Check if cell is in bounds
            if (!cell.InBounds(map))
            {
                reason = "Cell is out of bounds";
                return false;
            }
            
            // Check if cell has water
            var terrain = cell.GetTerrain(map);
            if (!terrain.HasTag("Water"))
            {
                reason = "This tile does not contain water";
                return false;
            }
            
            // Check water type compatibility if enforced
            if (FishEggsMod.Settings.enforceWaterType)
            {
                var terrainWaterType = GetTerrainWaterType(terrain);
                var fishWaterType = GetFishWaterType(fishDef);
                
                if (terrainWaterType != fishWaterType)
                {
                    string fishTypeStr = fishWaterType == WaterType.FreshWater ? "freshwater" : "saltwater";
                    reason = $"{fishDef.label.CapitalizeFirst()} eggs require {fishTypeStr}";
                    return false;
                }
            }
            
            return true;
        }
    }
}
