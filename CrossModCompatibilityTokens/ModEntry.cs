#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ContentPatcher;
using ContentPatcher.Framework;
using CrossModCompatibilityTokens.Helpers;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace CrossModCompatibilityTokens
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class ModEntry : Mod
    {
        internal static IMonitor ModMonitor { get; private set; } = null!;
        
        internal static IManifest Manifest { get; private set; } = null!;

        private static object ModRegistry { get; set; } = null!;

        internal static Dictionary<string, IMod> ModList { get; } = [];

        internal static Dictionary<string, IContentPack> PackList { get; } = [];

        internal static IContentPatcherAPI? ContentPatcherAPI { get; private set; }

        private static object? TokenManager { get; set; }
        private static object? LocalTokens { get; set; }

        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            Manifest = ModManifest;

            var SCore = typeof(Mod).Assembly.GetType("StardewModdingAPI.Framework.SCore")!.GetProperty("Instance",
                BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null);
            ModRegistry = AccessTools.Field(SCore!.GetType(), "ModRegistry")?.GetValue(SCore)!;
            
            Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            GrabMods();
            ContentPatcherAPI = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if (ContentPatcherAPI is null)
            {
                Log.Error("Content Patcher is not installed! This mod will not work without it.");
                return;
            }

            ContentPatcherAPI.RegisterToken(this.ModManifest, "Config", new ConfigToken());
            ContentPatcherAPI.RegisterToken(this.ModManifest, "Translation", new TranslationToken());
            ContentPatcherAPI.RegisterToken(this.ModManifest, "Dynamic", new DynamicToken());
            ContentPatcherAPI.RegisterToken(this.ModManifest, "Asset", new AssetToken());
        }
        
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (ContentPatcherAPI is not { IsConditionsApiReady: true }) return;
            GrabTokenManager();
            Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        }

        private static void GrabTokenManager()
        {
            var cpType = typeof(ContentPatcherAPI).Assembly.GetType("ContentPatcher.ModEntry");
            var cpMod = ModList["Pathoschild.ContentPatcher"];
            var PerScreenManager = AccessTools.Field(cpType, "ScreenManager").GetValue(cpMod);
            var screenManager = AccessTools.Property(PerScreenManager!.GetType(), "Value").GetValue(PerScreenManager);
            TokenManager = AccessTools.Property(screenManager?.GetType(), "TokenManager")?.GetValue(screenManager);
            LocalTokens = AccessTools.Field(TokenManager?.GetType(), "LocalTokens")?.GetValue(TokenManager);
        }

        public static object? GrabDynamicToken(string? uniqueID, string tokenKey)
        {
            if (LocalTokens is null || uniqueID is null) return null;
            try
            {
                var modCachedContext = AccessTools.Property(LocalTokens.GetType(), "Item")
                    ?.GetValue(LocalTokens, new object[] { uniqueID });
                var modContext = AccessTools.Property(modCachedContext!.GetType(), "Context")
                    ?.GetValue(modCachedContext);
                
                Log.Trace($"Grabbing token '{tokenKey}' from '{uniqueID}'");
                return modContext!.GetType().GetMethod("GetToken")?.Invoke(modContext, new object[] { tokenKey, true });
            } 
            catch (Exception e)
            {
                Log.Error($"Error grabbing dynamic token: {e}");
            }
            
            return null;
        }

        private static void GrabMods()
        {
            if (ModList.Any() || PackList.Any()) return;
            Log.Trace("Grabbing mods...");

            if (AccessTools.Method(ModRegistry.GetType(), "GetAll")?.Invoke(ModRegistry, new object[] { true, true }) is
                not IEnumerable<object> modList) return;

            foreach (var mod in modList)
            {
                if (AccessTools.Property(mod.GetType(), "Mod")?.GetValue(mod) is Mod rawMod)
                {
                    ModList.Add(rawMod.ModManifest.UniqueID, rawMod);
                    Log.Trace($"Tracking Mod: {rawMod.ModManifest.UniqueID}");
                }

                if (AccessTools.Property(mod.GetType(), "ContentPack")?.GetValue(mod) is IContentPack rawPack)
                {
                    PackList.Add(rawPack.Manifest.UniqueID, rawPack);
                    Log.Trace($"Tracking Content Pack: {rawPack.Manifest.UniqueID}");
                }
            }
        }

        public static JToken? GrabConfigValue(string uniqueID, string? valueToFind)
        {
            if (uniqueID.Equals(Manifest.UniqueID)) return null;
            var config = GrabConfig(uniqueID);
            if (config == null)
            {
                Log.Warn($"Mod with UniqueID '{uniqueID}' does not have any configuration options!");
                return null;
            }

            if (valueToFind is null)
            {
                Log.Trace("Tried to grab a config value without a value to find! This may happen at startup regardless, but double check your token names just in case.");
                return null;
            }

            var valueSplit = valueToFind.Split('.');
            var currentValue = config.GetValue(valueSplit[0]);
            
            if (valueSplit.Length == 1) return currentValue;

            for (var i = 1; i < valueSplit.Length; i++)
            {
                if (currentValue is not JObject currentObject)
                {
                    Log.Warn($"Config schema from '{uniqueID}' does not have a config matching '{valueToFind}'!");
                    return null;
                }
                currentValue = currentObject.GetValue(valueSplit[i]);
            }

            return currentValue;
        }

        private static JObject? GrabConfig(string uniqueID)
        {
            try
            {
                if (ModList.TryGetValue(uniqueID, out var mod))
                {
                    return mod.Helper.ModContent.Load<JObject>("config.json");
                }

                if (PackList.TryGetValue(uniqueID, out var pack))
                {
                    return pack.ReadJsonFile<JObject>("config.json");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error grabbing config for {uniqueID}: {e}");
            }

            return null;
        }

        public static ITranslationHelper? GrabTranslationHelper(string uniqueID)
        {
            try
            {
                if (ModList.TryGetValue(uniqueID, out var mod))
                {
                    return mod.Helper.Translation;
                }

                if (PackList.TryGetValue(uniqueID, out var pack))
                {
                    return pack.Translation;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error grabbing translation helper for {uniqueID}: {e}");
            }

            return null;
        }

        public static IAssetName? GrabInternalAssetName(string uniqueID, string path)
        {
            var modContentHelper = GrabModContentHelper(uniqueID);
            if (modContentHelper is null)
            {
                Log.Error($"Could not find mod content helper for '{uniqueID}'!");
                return null;
            }
            return modContentHelper.GetInternalAssetName(path);
        }

        public static IModContentHelper? GrabModContentHelper(string uniqueID)
        {
            try
            {
                if (PackList.TryGetValue(uniqueID, out var pack))
                {
                    return pack.ModContent;
                }

                if (ModList.TryGetValue(uniqueID, out var mod))
                {
                    return mod.Helper.ModContent;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error grabbing mod content helper for {uniqueID}: {e}");
            }
            
            return null;
        }
    }
}