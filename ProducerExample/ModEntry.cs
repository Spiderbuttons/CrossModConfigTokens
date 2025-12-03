using System;
using System.Reflection;
using CrossModCompatibilityTools.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using ProducerExample.Helpers;
using StardewValley.Internal;

namespace ProducerExample
{
    internal sealed class ModEntry : Mod
    {
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static IMonitor ModMonitor { get; set; } = null!;
        
        internal static ICrossModCompatibilityToolsAPI api = null!;

        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            ModMonitor = Monitor;

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void Tester(Delegate obj)
        {
            Log.Debug("tester");
            obj.DynamicInvoke();
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            api = Helper.ModRegistry.GetApi<ICrossModCompatibilityToolsAPI>("Spiderbuttons.CMCT")!;

            if (!api.TryRegisterAction(
                    manifest: ModManifest,
                    actionId: "TestAction",
                    category: "Test",
                    name: () => "The name of this action goes here.",
                    description: () => "This action logs a little test log to the SMAPI console!",
                    action: () => Log.Error("This doesn't do much."),
                    customFields: null,
                    customData: new TestData("UniqueId/AssetName", () => "An icon for my whatever.", null),
                    error: out var error))
            {
                Log.Error(error);
            }

            Log.Info("Successfully registered TestAction.");

            var otherMod = ModHelper.ModRegistry.Get("Spiderbuttons.ButtonsExtraBooksCore")!;
            if (!api.TryRegisterAction(otherMod.Manifest, TestAction, out error))
            {
                Log.Error(error);
            }
            
            Log.Info("Successfully registered BookAction.");
            
            Tester(TestAction);
        }
        
        public void TestAction()
        {
            ModMonitor.Log("This is a test action!", LogLevel.Alert);
        }
    }

    public class TestData(string assetName, Func<string> description, Texture2D? texture)
    {
        public string AssetName { get; set; } = assetName;
        public Func<string> Description { get; set; } = description;
        public Texture2D? Texture { get; set; } = texture;

        public void ExampleFunction(int value)
        {
            Log.Debug($"Example. {value}");
        }
    }
}