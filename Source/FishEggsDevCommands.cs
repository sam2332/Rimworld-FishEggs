using System.Linq;
using RimWorld;
using Verse;

namespace FishEggs
{
    /// <summary>
    /// DEV mode commands for testing fish eggs
    /// </summary>
    public static class FishEggsDevCommands
    {
        [DebugAction("Fish Eggs", "Spawn All Fish Eggs")]
        public static void SpawnAllFishEggs()
        {
            var map = Find.CurrentMap;
            var player = Find.ColonistBar.GetColonistsInOrder().FirstOrDefault();
            if (player == null)
            {
                Messages.Message("No colonist found", MessageTypeDefOf.RejectInput);
                return;
            }

            var fishEggDefs = DefDatabase<ThingDef>.AllDefs
                .Where(d => d.thingClass == typeof(ThingWithComps_FishEgg))
                .ToList();

            if (!fishEggDefs.Any())
            {
                Messages.Message("No fish egg defs found - check generation", MessageTypeDefOf.RejectInput);
                return;
            }

            var spawnPos = player.Position + IntVec3.North;
            
            foreach (var eggDef in fishEggDefs)
            {
                var egg = ThingMaker.MakeThing(eggDef);
                egg.stackCount = 5;
                GenPlace.TryPlaceThing(egg, spawnPos, map, ThingPlaceMode.Near);
                spawnPos += IntVec3.East;
            }

            Messages.Message($"Spawned {fishEggDefs.Count} types of fish eggs", MessageTypeDefOf.PositiveEvent);
        }

        [DebugAction("Fish Eggs", "Log Fish Discovery")]
        public static void LogFishDiscovery()
        {
            var fishDefs = FishEggUtility.GetAllFishDefs();
            Log.Message($"[FishEggs] Found {fishDefs.Count} fish types:");
            
            foreach (var fish in fishDefs)
            {
                var waterType = FishEggUtility.GetFishWaterType(fish);
                Log.Message($"  {fish.defName} ({fish.label}) - {waterType}");
            }
        }

        [DebugAction("Fish Eggs", "Test Water Detection")]
        public static void TestWaterDetection()
        {
            var map = Find.CurrentMap;
            var waterCells = map.AllCells.Where(c => c.GetTerrain(map).HasTag("Water")).Take(10);
            
            foreach (var cell in waterCells)
            {
                var terrain = cell.GetTerrain(map);
                var waterType = FishEggUtility.GetTerrainWaterType(terrain);
                Log.Message($"Cell {cell}: {terrain.defName} -> {waterType}");
            }
        }
    }
}
