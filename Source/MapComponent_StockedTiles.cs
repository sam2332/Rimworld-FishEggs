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
                }
                catch (System.Exception ex)
                {
                    Log.Warning($"[FishEggs] Could not add fish to water body via reflection: {ex.Message}");
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
            
            if (stockedTiles == null)
            {
                stockedTiles = new Dictionary<IntVec3, ThingDef>();
            }
        }
    }
}
