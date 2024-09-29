#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CrossModConfigTokens.Helpers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace CrossModConfigTokens
{
    internal sealed class ModEntry : Mod
    {
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static IMonitor ModMonitor { get; set; } = null!;
        
        internal static object ModRegistry { get; set; } = null!;
        
        internal static Dictionary<string, IMod> ModList { get; set; } = [];

        internal static Dictionary<string, IContentPack> PackList { get; set; } = [];

        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            ModMonitor = Monitor;
            
            var SCore = typeof(Mod).Assembly.GetType("StardewModdingAPI.Framework.SCore")!.GetProperty("Instance",
                BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null);
            ModRegistry = AccessTools.Field(SCore!.GetType(), "ModRegistry")?.GetValue(SCore)!;
            

            Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        public static void GrabMods()
        {
            if (ModList.Any() || PackList.Any()) return;
            Log.Alert("Grabbing mods...");

            if (AccessTools.Method(ModRegistry.GetType(), "GetAll")?.Invoke(ModRegistry, new object[] { true, true }) is not IEnumerable<object> modList) return;
                
            foreach (var mod in modList)
            {
                if (AccessTools.Property(mod.GetType(), "Mod")?.GetValue(mod) is IMod rawMod)
                {
                    ModList.Add(rawMod.ModManifest.UniqueID, rawMod);
                    Log.Warn($"Adding Mod: {rawMod.ModManifest.UniqueID}");
                }

                if (AccessTools.Property(mod.GetType(), "ContentPack")?.GetValue(mod) is IContentPack rawPack)
                {
                    PackList.Add(rawPack.Manifest.UniqueID, rawPack);
                    Log.Warn($"Adding Pack: {rawPack.Manifest.UniqueID}");
                }
            }
        }

        public static void GrabConfig(IMod mod)
        {
            var config = mod.Helper.ReadConfig<Dictionary<string, object>>();
            
            Log.Error(mod.ModManifest.UniqueID);
            foreach (var kvp in config)
            {
                if (!kvp.Value.GetType().IsPrimitive)
                {
                    Log.Error("Found an object config!");
                }
                Log.Alert($"{kvp.Key}: {kvp.Value}");
            }
        }

        public static void GrabConfig(IContentPack mod)
        {
            var config = mod.ReadJsonFile<Dictionary<string, object>>("config.json");
            if (config == null) return;
            
            Log.Error(mod.Manifest.UniqueID);
            foreach (var kvp in config)
            {
                Log.Error(kvp.Value.GetType());
                Log.Alert($"{kvp.Key}: {kvp.Value}");
            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // if (!Context.IsWorldReady)
            //     return;

            if (e.Button is SButton.F5) GrabMods();
            
            if (e.Button is SButton.F6) GrabConfig(ModList["Pathoschild.LookupAnything"]);
        }
    }
}