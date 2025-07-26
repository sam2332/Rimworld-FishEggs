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
                selectable = true, // This makes the item clickable!
                useHitPoints = true,
                altitudeLayer = AltitudeLayer.Item,
                drawerType = DrawerType.MapMeshOnly,
                
                graphicData = new GraphicData
                {
                    texPath = "Things/Item/Resource/AnimalProductRaw/EggRound", // Use vanilla egg texture
                    graphicClass = typeof(Graphic_StackCount),
                    color = GenerateConsistentEggColor(fishDef.defName) // Pseudo-random but consistent color based on fish name
                },
                
                statBases = new List<StatModifier>
                {
                    new StatModifier { stat = StatDefOf.MaxHitPoints, value = 25 },
                    new StatModifier { stat = StatDefOf.MarketValue, value = fishDef.GetStatValueAbstract(StatDefOf.MarketValue) * 2f },
                    new StatModifier { stat = StatDefOf.Mass, value = 0.1f },
                    new StatModifier { stat = StatDefOf.Flammability, value = 0.5f },
                    new StatModifier { stat = StatDefOf.DeteriorationRate, value = 1f }
                },
                
                soundDrop = SoundDefOf.Standard_Drop,
                soundPickup = SoundDefOf.Standard_Pickup,
                soundInteract = SoundDefOf.Interact_Sow,
                thingCategories = new List<ThingCategoryDef>(),
                stackLimit = 100,
                drawGUIOverlay = true,
                alwaysHaulable = true,
                pathCost = 1,
                
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
            
            // Set category after creation using a safe method
            var animalProductCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail("AnimalProductRaw");

            if (animalProductCategory != null)
            {
                // Always add AnimalProductRaw as well, so eggs are storable anywhere animal products are accepted
                eggDef.thingCategories.Add(animalProductCategory);
            }
            if (animalProductCategory == null)
            {
                Log.Error($"[FishEggs] AnimalProductRaw category not found for {eggDef.defName}, item may not appear in categories");
            }
            
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
        
        private static UnityEngine.Color GenerateConsistentEggColor(string fishDefName)
        {
            // Use the fish defName to generate a consistent hash
            var hash = fishDefName.GetHashCode();
            
            // Use different parts of the hash for different color components
            var random1 = (uint)(hash & 0xFFFF);
            var random2 = (uint)((hash >> 8) & 0xFFFF);
            var random3 = (uint)((hash >> 16) & 0xFFFF);
            
            // Generate color components in ranges that look good for eggs
            // Keep them in egg-like color ranges (pastels, earth tones)
            var hue = (random1 % 360) / 360f;
            var saturation = 0.3f + ((random2 % 100) / 100f) * 0.4f; // 0.3 to 0.7
            var brightness = 0.7f + ((random3 % 100) / 100f) * 0.25f; // 0.7 to 0.95
            
            // Convert HSV to RGB for more natural color distribution
            return UnityEngine.Color.HSVToRGB(hue, saturation, brightness);
        }
    }
}
