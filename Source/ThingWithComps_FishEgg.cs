using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FishEggs
{
    /// <summary>
    /// Thing class for fish eggs
    /// </summary>
    public class ThingWithComps_FishEgg : ThingWithComps
    {
        public ThingDef LinkedFishDef => def.GetModExtension<FishEggProperties>()?.linkedFishDef;
        public WaterType RequiredWaterType => def.GetModExtension<FishEggProperties>()?.requiredWaterType ?? WaterType.FreshWater;
        
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var option in base.GetFloatMenuOptions(selPawn))
            {
                yield return option;
            }
            
            // Add seeding option if enabled
            if (FishEggsMod.Settings.enableContextMenuSeeding && LinkedFishDef != null)
            {
                yield return new FloatMenuOption("Seed Water Source", () =>
                {
                    Find.Targeter.BeginTargeting(
                        TargetingParameters.ForCell(), 
                        (LocalTargetInfo target) =>
                        {
                            var job = JobMaker.MakeJob(FishEggsDefOf.SeedWaterSource, this, target);
                            job.count = 1; // Set count to 1 since we use exactly 1 fish egg
                            selPawn.jobs.TryTakeOrderedJob(job);
                        },
                        selPawn
                    );
                });
            }
        }
    }
    
    /// <summary>
    /// Mod extension to store fish egg properties
    /// </summary>
    public class FishEggProperties : DefModExtension
    {
        public ThingDef linkedFishDef;
        public WaterType requiredWaterType;
    }
}
