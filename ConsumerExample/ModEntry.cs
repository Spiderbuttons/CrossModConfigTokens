using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using ConsumerExample.Helpers;
using CrossModCompatibilityTools.API;
using Microsoft.Xna.Framework.Graphics;

namespace ConsumerExample
{
    internal sealed class ModEntry : Mod
    {
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static IMonitor ModMonitor { get; set; } = null!;
        
        internal static ICrossModCompatibilityToolsAPI api = null!;

        internal static IReadOnlyList<ICrossModAction>? actions = null!;

        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            ModMonitor = Monitor;

            Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }
        
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            api = Helper.ModRegistry.GetApi<ICrossModCompatibilityToolsAPI>("Spiderbuttons.CMCT")!;

            if (!api.TryGetActionsAsList(out actions, out var error))
            {
                ModMonitor.Log(error, LogLevel.Error);
                return;
            }

            foreach (var action in actions)
            {
                ModMonitor.Log($"Action '{action.Id}' registered by {action.Mod.Manifest.UniqueID}.", LogLevel.Debug);
            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button is SButton.F3 && actions is not null)
            {
                var data = actions.First().GetCustomData<SomeTestData>();
                ModMonitor.Log($"Data: {data?.AssetName} - {data?.Description()}", LogLevel.Info);
                actions.First().PerformAction();
                data?.ExampleFunction(4);
            }
        }
    }
    
    public interface SomeTestData
    {
        public string AssetName { get; set; }
        public Func<string> Description { get; set; }
        public Texture2D? Texture { get; set; }

        public void ExampleFunction(int value);
    }
}