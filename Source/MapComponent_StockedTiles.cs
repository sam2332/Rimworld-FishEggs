using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace FishEggs
{
    /// <summary>
    /// MapComponent that uses vanilla WaterBody fish tracking for stocked tiles
    /// </summary>
    public class MapComponent_StockedTiles : MapComponent
    {
        private Dictionary<IntVec3, ThingDef> stockedTiles = new Dictionary<IntVec3, ThingDef>();
        private Dictionary<IntVec3, int> bonusCapacityByCell = new Dictionary<IntVec3, int>();
        
        public MapComponent_StockedTiles(Map map) : base(map)
        {
        }
        
        /// <summary>
        /// Mark a tile as stocked with a specific fish type using vanilla water body system
        /// </summary>
        public void MarkTileStocked(IntVec3 cell, ThingDef fishDef)
        {
            stockedTiles[cell] = fishDef;

            // Add fish to vanilla water body system using reflection
            var waterBody = map.waterBodyTracker?.WaterBodyAt(cell);
            if (waterBody != null)
            {
                try
                {
                    // Use reflection to access private commonFish field
                    var commonFishField = typeof(WaterBody).GetField("commonFish", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (commonFishField != null)
                    {
                        var commonFish = (List<ThingDef>)commonFishField.GetValue(waterBody);
                        if (commonFish != null && !commonFish.Contains(fishDef))
                        {
                            commonFish.Add(fishDef);
                            Log.Message($"[FishEggs] Added {fishDef.label} to water body at {cell}");
                        }
                    }

                    // Instead of modifying current population, we need to increase the maximum capacity
                    // We'll do this by artificially increasing the water body size which affects PopulationFactor
                    var cellCountField = typeof(WaterBody).GetField("cellCount", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (cellCountField != null)
                    {
                        int currentCellCount = (int)cellCountField.GetValue(waterBody);
                        // Add equivalent of extra cells to increase carrying capacity
                        // Each "virtual cell" adds carrying capacity based on the population factor curve
                        int bonusCells = 100; // This increases population capacity significantly
                        
                        // Track the bonus capacity for this cell so we can restore it after map reload
                        bonusCapacityByCell[cell] = bonusCells;
                        
                        cellCountField.SetValue(waterBody, currentCellCount + bonusCells);
                        
                        // Force recalculation of cached population factor
                        var cachedPopulationFactorField = typeof(WaterBody).GetField("cachedPopulationFactor", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (cachedPopulationFactorField != null)
                        {
                            cachedPopulationFactorField.SetValue(waterBody, -1f); // Force recalculation
                        }
                        
                        Log.Message($"[FishEggs] Increased water body capacity by adding {bonusCells} virtual cells at {cell} (cellCount: {currentCellCount} -> {currentCellCount + bonusCells})");
                        Log.Message($"[FishEggs] New max population: {waterBody.MaxPopulation}");
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Warning($"[FishEggs] Could not add fish to water body or increase population via reflection: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Check if a tile is stocked and return the fish type
        /// </summary>
        public ThingDef GetStockedFish(IntVec3 cell)
        {
            stockedTiles.TryGetValue(cell, out ThingDef fishDef);
            return fishDef;
        }
        
        /// <summary>
        /// Check if a tile is stocked
        /// </summary>
        public bool IsTileStocked(IntVec3 cell)
        {
            return stockedTiles.ContainsKey(cell);
        }
        
        /// <summary>
        /// Remove stocking from a tile
        /// </summary>
        public void RemoveStocking(IntVec3 cell)
        {
            stockedTiles.Remove(cell);
            bonusCapacityByCell.Remove(cell);
        }
        
        /// <summary>
        /// Restore bonus capacity to all stocked water bodies after map load
        /// </summary>
        public void RestoreBonusCapacity()
        {
            foreach (var kvp in bonusCapacityByCell)
            {
                var cell = kvp.Key;
                var bonusCells = kvp.Value;
                
                var waterBody = map.waterBodyTracker?.WaterBodyAt(cell);
                if (waterBody != null)
                {
                    try
                    {
                        var cellCountField = typeof(WaterBody).GetField("cellCount", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (cellCountField != null)
                        {
                            int currentCellCount = (int)cellCountField.GetValue(waterBody);
                            cellCountField.SetValue(waterBody, currentCellCount + bonusCells);
                            
                            // Force recalculation of cached population factor
                            var cachedPopulationFactorField = typeof(WaterBody).GetField("cachedPopulationFactor", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (cachedPopulationFactorField != null)
                            {
                                cachedPopulationFactorField.SetValue(waterBody, -1f);
                            }
                            
                            Log.Message($"[FishEggs] Restored {bonusCells} bonus capacity to water body at {cell} (new cellCount: {currentCellCount + bonusCells})");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Warning($"[FishEggs] Could not restore bonus capacity to water body at {cell}: {ex.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Get all stocked tiles
        /// </summary>
        public IEnumerable<IntVec3> GetAllStockedTiles()
        {
            return stockedTiles.Keys;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref stockedTiles, "stockedTiles", LookMode.Value, LookMode.Def);
            Scribe_Collections.Look(ref bonusCapacityByCell, "bonusCapacityByCell", LookMode.Value, LookMode.Value);
            
            if (stockedTiles == null)
            {
                stockedTiles = new Dictionary<IntVec3, ThingDef>();
            }
            
            if (bonusCapacityByCell == null)
            {
                bonusCapacityByCell = new Dictionary<IntVec3, int>();
            }
            
            // After loading, restore the bonus capacity
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                RestoreBonusCapacity();
            }
        }
    }
}
