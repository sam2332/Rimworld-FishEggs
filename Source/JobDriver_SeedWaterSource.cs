using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FishEggs
{
    /// <summary>
    /// Job driver for seeding water sources with fish eggs
    /// </summary>
    public class JobDriver_SeedWaterSource : JobDriver
    {
        private const TargetIndex EggIndex = TargetIndex.A;
        private const TargetIndex CellIndex = TargetIndex.B;
        
        protected Thing FishEgg => job.GetTarget(EggIndex).Thing;
        protected IntVec3 TargetCell => job.GetTarget(CellIndex).Cell;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(FishEgg, job, 1, -1, null, errorOnFailed);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Validate the egg and cell
            this.FailOnDestroyedNullOrForbidden(EggIndex);
            this.FailOnBurningImmobile(CellIndex);
            
            // Go to the egg
            yield return Toils_Goto.GotoThing(EggIndex, PathEndMode.Touch);
            
            // Pick up the egg
            yield return Toils_Haul.StartCarryThing(EggIndex);
            
            // Go to the target cell
            yield return Toils_Goto.GotoCell(CellIndex, PathEndMode.Touch);
            
            // Perform the seeding
            var seedToil = new Toil
            {
                initAction = () =>
                {
                    var fishEgg = FishEgg as ThingWithComps_FishEgg;
                    if (fishEgg?.LinkedFishDef == null)
                    {
                        Log.Error("[FishEggs] Fish egg has no linked fish def");
                        EndJobWith(JobCondition.Errored);
                        return;
                    }
                    
                    // Validate the cell
                    if (!FishEggUtility.IsValidSeedingCell(TargetCell, pawn.Map, fishEgg.LinkedFishDef, out string reason))
                    {
                        Messages.Message(reason, MessageTypeDefOf.RejectInput);
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }
                    
                    // Perform the seeding
                    var mapComponent = pawn.Map.GetComponent<MapComponent_StockedTiles>();
                    mapComponent.MarkTileStocked(TargetCell, fishEgg.LinkedFishDef);
                    
                    // Grant Animals XP
                    if (FishEggsMod.Settings.animalsXpPerSeeding > 0)
                    {
                        pawn.skills?.Learn(SkillDefOf.Animals, FishEggsMod.Settings.animalsXpPerSeeding);
                    }
                    
                    // Show success message
                    Messages.Message($"{pawn.LabelShort} successfully seeded {TargetCell} with {fishEgg.LinkedFishDef.label}", 
                        MessageTypeDefOf.PositiveEvent);
                    
                    // Show ripple effect if enabled
                    if (FishEggsMod.Settings.showRippleOnSuccess)
                    {
                        FleckMaker.WaterSplash(TargetCell.ToVector3(), pawn.Map, 1.5f, 1f);
                    }
                    
                    // Consume the egg
                    fishEgg.Destroy();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            
            yield return seedToil;
        }
    }
}
