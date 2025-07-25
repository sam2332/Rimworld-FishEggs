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
        static FishEggDefGenerator()
        {
            GenerateFishEggDefs();
        }
        
        private static void GenerateFishEggDefs()
        {
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
        
        private static ThingDef CreateFishEggDef(ThingDef fishDef)
        {
            var waterType = FishEggUtility.GetFishWaterType(fishDef);
            
            var eggDef = new ThingDef
            {
                defName = $"FishEgg_{fishDef.defName}",
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
            
            // Generate unique short hash for the def
            eggDef.shortHash = 0;
            eggDef.shortHash = GiveShortHash(eggDef, typeof(ThingDef));
            
            return eggDef;
        }
        
        // Helper method to generate short hash (adapted from RimWorld's method)
        private static ushort GiveShortHash(Def def, System.Type defType)
        {
            var hashCode = (def.defName + defType.ToString()).GetHashCode();
            return (ushort)(hashCode & 0xFFFF);
        }
    }
}
