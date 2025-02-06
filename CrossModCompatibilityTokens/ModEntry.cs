#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using CrossModCompatibilityTokens.Helpers;
using CrossModCompatibilityTokens.Integration;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using CrossModCompatibilityTokens.Tokens;

namespace CrossModCompatibilityTokens
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class ModEntry : Mod
    {
        internal static IMonitor ModMonitor { get; private set; } = null!;
        
        internal static IModHelper ModHelper { get; private set; } = null!;
        
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
            ModHelper = helper;
            Manifest = ModManifest;

            var SCore = typeof(Mod).Assembly.GetType("StardewModdingAPI.Framework.SCore")!.GetProperty("Instance",
                BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null);
            ModRegistry = AccessTools.Field(SCore!.GetType(), "ModRegistry")?.GetValue(SCore)!;
            
            Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button is SButton.F2)
            {
                if (!Registrar.TryGetContentPack(Helper.ModRegistry.GetAll().First(p => p.IsContentPack), out IContentPack? val, out string? error))
                {
                    Log.Error(error);
                    return;
                }

                Log.Alert(val.Manifest.UniqueID);
            }
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
            if (ContentPatcherAPI is not null and not { IsConditionsApiReady: true }) return;
            GrabTokenManager();
            Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        }

        private static void GrabTokenManager()
        {
            var cpMod = ModList["Pathoschild.ContentPatcher"];
            var cpType = cpMod.GetType();//.Assembly.GetType("ContentPatcher.ModEntry");
            Log.Debug(cpType);
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