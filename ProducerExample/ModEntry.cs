using System;
using CrossModCompatibilityTools.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using ProducerExample.Helpers;

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

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            api = Helper.ModRegistry.GetApi<ICrossModCompatibilityToolsAPI>("Spiderbuttons.CMCT")!;

            if (!api.TryRegisterAction(ModManifest, "TestAction", "Test",
                    () => "This action logs a little test log to the SMAPI console!", TestAction, null,
                    new TestData("UniqueId/AssetName", () => "An icon for my whatever.", null), out var error))
            {
                ModMonitor.Log(error, LogLevel.Error);
            }

            ModMonitor.Log("Successfully registered TestAction.", LogLevel.Info);
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
    }
}