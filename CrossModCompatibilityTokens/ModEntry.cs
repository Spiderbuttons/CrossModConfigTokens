#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using CrossModCompatibilityTokens.Helpers;
using CrossModCompatibilityTokens.Integration;
using CrossModCompatibilityTokens.Readers;
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

        internal static IContentPatcherAPI? ContentPatcherAPI { get; private set; }

        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            ModHelper = helper;
            Manifest = ModManifest;
            
            Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button is SButton.F2)
            {
                if (!DynamicReader.TryGetDynamicTokenValues("Test.Mod", "Style", out var values, out var error))
                {
                    Log.Error(error);
                }

                foreach (var value in values ?? Enumerable.Empty<string>())
                {
                    Log.Debug(value);
                }
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            ContentPatcherAPI = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if (ContentPatcherAPI is null)
            {
                Log.Error("Content Patcher is not installed! This mod will not work without it.");
                return;
            }

            ContentPatcherAPI.RegisterToken(this.ModManifest, "Config", new ConfigToken());
            ContentPatcherAPI.RegisterToken(this.ModManifest, "Translation", new TranslationToken());
            ContentPatcherAPI.RegisterToken(this.ModManifest, "Dynamic", new DynamicToken());
            ContentPatcherAPI.RegisterToken(this.ModManifest, "Asset", new InternalAssetToken());
            ContentPatcherAPI.RegisterToken(this.ModManifest, "InternalAsset", new InternalAssetToken());
        }
    }
}