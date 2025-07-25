using RimWorld;
using Verse;

namespace FishEggs
{
    /// <summary>
    /// DefOf class for Fish Eggs mod
    /// </summary>
    [DefOf]
    public static class FishEggsDefOf
    {
        public static JobDef SeedWaterSource;
        
        static FishEggsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FishEggsDefOf));
        }
    }
}
