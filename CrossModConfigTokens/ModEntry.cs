#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CrossModConfigTokens.Helpers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
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
            Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }
        
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            GrabMods();
        }

        private static void GrabMods()
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

        public static JToken? GrabConfigValue(string uniqueID, string? valueToFind)
        {
            var config = GrabConfig(uniqueID);
            if (config == null) return null;

            if (valueToFind is null)
            {
                return null;
            }
            
            var valueSplit = valueToFind.Split('.');
            var currentValue = config.GetValue(valueSplit[0]);
            if (valueSplit.Length == 1) return currentValue;
            
            for (var i = 1; i < valueSplit.Length; i++)
            {
                if (currentValue is not JObject currentObject) return null;
                currentValue = currentObject.GetValue(valueSplit[i]);
            }
            
            return currentValue;
        }

        private static JObject? GrabConfig(string uniqueID)
        {
            try
            {
                if (ModList.TryGetValue(uniqueID, out var mod1))
                {
                    return mod1.Helper.ModContent.Load<JObject>("config.json");
                }

                if (PackList.TryGetValue(uniqueID, out var mod))
                {
                    return mod.ReadJsonFile<JObject>("config.json");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error grabbing config for {uniqueID}: {e}");
            }

            return null;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // if (!Context.IsWorldReady)
            //     return;

            if (e.Button is SButton.F7) Log.Warn(GrabConfigValue("Spiderbuttons.ButtonsExtraBooksCore", "CheatCodesPrice")?.Value<string>());
        }
    }
}