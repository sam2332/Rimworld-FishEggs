using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FishEggs
{
    /// <summary>
    /// Generates fish egg ThingDefs dynamically for all discovered fish
    /// </summary>
    [StaticConstructorOnStartup]
    public static class FishEggDefGenerator
    {
        private static HashSet<ushort> usedShortHashes = new HashSet<ushort>();
        
        static FishEggDefGenerator()
        {
            // Delay generation to ensure all other mods have loaded their definitions
            LongEventHandler.QueueLongEvent(GenerateFishEggDefs, "Generating fish egg definitions...", false, null);
        }
        
        private static void GenerateFishEggDefs()
        {
            // Collect all existing short hashes to avoid collisions
            CollectExistingShortHashes();
            
            var fishDefs = FishEggUtility.GetAllFishDefs();
            var generatedCount = 0;
            
            foreach (var fishDef in fishDefs)
            {
                var eggDef = CreateFishEggDef(fishDef);
                if (eggDef != null)
                {
                    DefDatabase<ThingDef>.Add(eggDef);
                    generatedCount++;
                }
            }
            
            Log.Message($"[FishEggs] Generated {generatedCount} fish egg definitions");
        }
        
        private static void CollectExistingShortHashes()
        {
            usedShortHashes.Clear();
            
            // Collect all existing ThingDef short hashes
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                usedShortHashes.Add(thingDef.shortHash);
            }
            
            Log.Message($"[FishEggs] Collected {usedShortHashes.Count} existing short hashes to avoid collisions");
        }
        
        private static ThingDef CreateFishEggDef(ThingDef fishDef)
        {
            var waterType = FishEggUtility.GetFishWaterType(fishDef);
            
            // Use a simpler, more consistent defName format
            // This provides better backward compatibility with save games
            var eggDefName = $"FishEgg_{fishDef.defName}";
            
            // Check if this def already exists (shouldn't happen, but safety first)
            if (DefDatabase<ThingDef>.GetNamedSilentFail(eggDefName) != null)
            {
                Log.Warning($"[FishEggs] Fish egg def {eggDefName} already exists, skipping generation for {fishDef.defName}");
                return null;
            }
            
            var eggDef = new ThingDef
            {
                defName = eggDefName,
                label = $"{fishDef.label} egg",
                description = $"An egg containing {fishDef.label} spawn. Can be used to seed {(waterType == WaterType.FreshWater ? "freshwater" : "saltwater")} sources.",
                
                thingClass = typeof(ThingWithComps_FishEgg),
                category = ThingCategory.Item,
                
                graphicData = new GraphicData
                {
                    texPath = "Things/Item/FishEgg/FishEgg_Generic", // We'll need to create this texture
                    graphicClass = typeof(Graphic_StackCount)
                },
                
                statBases = new List<StatModifier>
                {
                    new StatModifier { stat = StatDefOf.MaxHitPoints, value = 25 },
                    new StatModifier { stat = StatDefOf.MarketValue, value = fishDef.GetStatValueAbstract(StatDefOf.MarketValue) * 2f },
                    new StatModifier { stat = StatDefOf.Mass, value = 0.1f },
                    new StatModifier { stat = StatDefOf.Flammability, value = 0.5f },
                    new StatModifier { stat = StatDefOf.DeteriorationRate, value = 1f }
                },
                
                // Add sound definitions to prevent null sound errors
                soundInteract = SoundDefOf.Standard_Drop,
                soundDrop = SoundDefOf.Standard_Drop,
                
                thingCategories = new List<ThingCategoryDef> { DefDatabase<ThingCategoryDef>.GetNamed("FishEggs") },
                stackLimit = 10,
                drawGUIOverlay = true,
                alwaysHaulable = true,
                pathCost = 14,
                
                comps = new List<CompProperties>
                {
                    new CompProperties_Rottable
                    {
                        daysToRotStart = 5,
                        rotDestroys = true
                    }
                },
                
                modExtensions = new List<DefModExtension>
                {
                    new FishEggProperties
                    {
                        linkedFishDef = fishDef,
                        requiredWaterType = waterType
                    }
                }
            };
            
            // Generate a unique short hash that doesn't collide with existing definitions
            eggDef.shortHash = GenerateUniqueShortHash(eggDef.defName);
            
            return eggDef;
        }
        
        private static ushort GenerateUniqueShortHash(string defName)
        {
            // Start with a hash based on the defName
            var baseHash = (ushort)(defName.GetHashCode() & 0xFFFF);
            var hash = baseHash;
            var attempts = 0;
            
            // If there's a collision, increment until we find a free hash
            while (usedShortHashes.Contains(hash) && attempts < 65536)
            {
                hash = (ushort)((hash + 1) & 0xFFFF);
                attempts++;
            }
            
            if (attempts >= 65536)
            {
                Log.Error($"[FishEggs] Could not generate unique short hash for {defName} after 65536 attempts!");
                return baseHash; // Fall back to base hash even if it collides
            }
            
            // Reserve this hash
            usedShortHashes.Add(hash);
            
            if (attempts > 0)
            {
                Log.Message($"[FishEggs] Generated unique short hash {hash} for {defName} (after {attempts} collision resolution attempts)");
            }
            
            return hash;
        }
    }
}
