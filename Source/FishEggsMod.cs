using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace FishEggs
{
    /// <summary>
    /// Main mod class that initializes the Fish Eggs mod
    /// </summary>
    public class FishEggsMod : Mod
    {
        public static FishEggsSettings Settings;
        
        public FishEggsMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<FishEggsSettings>();
            
            // Apply Harmony patches
            var harmony = new Harmony("FishEggs.ProgrammerLily.com");
            harmony.PatchAll();
            
            // Schedule fish egg generation after all defs are loaded
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                FishEggDefGenerator.Generate();
            });
            
            Log.Message("[FishEggs] Mod initialized successfully");
        }
        
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoWindowContents(inRect);
        }
        
        public override string SettingsCategory()
        {
            return "Fish Eggs";
        }
    }
}
