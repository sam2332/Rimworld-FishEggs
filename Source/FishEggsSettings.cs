using UnityEngine;
using Verse;

namespace FishEggs
{
    /// <summary>
    /// Mod settings for Fish Eggs
    /// </summary>
    public class FishEggsSettings : ModSettings
    {
        // Context Menu Settings
        public bool enableContextMenuSeeding = true;
        public bool enforceWaterType = true;
        public bool showRippleOnSuccess = true;
        
        // Water Type Settings
        public WaterType defaultWaterTypeForUntagged = WaterType.FreshWater;
        
        // Fishing Drop Settings
        public float fishingDropChance = 0.0005f; // 0.05%
        
        // XP Settings
        public int animalsXpPerSeeding = 50;
        
        // Trader Settings
        public bool traderEggRarityBoost = true;
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableContextMenuSeeding, "enableContextMenuSeeding", true);
            Scribe_Values.Look(ref enforceWaterType, "enforceWaterType", true);
            Scribe_Values.Look(ref showRippleOnSuccess, "showRippleOnSuccess", true);
            Scribe_Values.Look(ref defaultWaterTypeForUntagged, "defaultWaterTypeForUntagged", WaterType.FreshWater);
            Scribe_Values.Look(ref fishingDropChance, "fishingDropChance", 0.0005f);
            Scribe_Values.Look(ref animalsXpPerSeeding, "animalsXpPerSeeding", 50);
            Scribe_Values.Look(ref traderEggRarityBoost, "traderEggRarityBoost", true);
            base.ExposeData();
        }
        
        public void DoWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            
            // Context Menu Section
            listing.CheckboxLabeled("Enable Context Menu Seeding", ref enableContextMenuSeeding, 
                "Enables 'Seed Water Source' float menu on fish eggs");
            listing.Gap();
            
            listing.CheckboxLabeled("Enforce Correct Water Type", ref enforceWaterType, 
                "Blocks seeding if terrain type doesn't match fish egg requirements");
            listing.Gap();
            
            listing.CheckboxLabeled("Show Ripple on Success", ref showRippleOnSuccess, 
                "Shows visual effect when seeding succeeds");
            listing.Gap();
            
            // Water Type Settings
            listing.Label($"Default Water Type for Untagged Terrains: {defaultWaterTypeForUntagged}");
            if (listing.ButtonText($"Change to {(defaultWaterTypeForUntagged == WaterType.FreshWater ? "Salt Water" : "Fresh Water")}"))
            {
                defaultWaterTypeForUntagged = defaultWaterTypeForUntagged == WaterType.FreshWater ? WaterType.SaltWater : WaterType.FreshWater;
            }
            listing.Gap();
            
            // Fishing Drop Settings
            listing.Label($"Fishing Drop Chance for Fish Eggs: {fishingDropChance:P2}");
            fishingDropChance = listing.Slider(fishingDropChance, 0.0001f, 0.01f);
            listing.Gap();
            
            // XP Settings
            listing.Label($"Animals XP Gained per Seeding: {animalsXpPerSeeding}");
            animalsXpPerSeeding = (int)listing.Slider(animalsXpPerSeeding, 0, 500);
            listing.Gap();
            
            // Trader Settings
            listing.CheckboxLabeled("Trader Egg Rarity Boost", ref traderEggRarityBoost, 
                "Multiplies chance of traders carrying eggs");
            
            listing.End();
        }
    }
    
    /// <summary>
    /// Water type enumeration for fish eggs
    /// </summary>
    public enum WaterType
    {
        FreshWater,
        SaltWater
    }
}
