using CrossModCompatibilityTokens.API;
using CrossModCompatibilityTokens.Helpers;
using CrossModCompatibilityTokens.Integration;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using CrossModCompatibilityTokens.Tokens;

namespace CrossModCompatibilityTokens
{
    internal sealed class ModEntry : Mod
    {
        internal static IMonitor ModMonitor { get; private set; } = null!;
        internal static IModHelper ModHelper { get; private set; } = null!;
        private static IManifest Manifest { get; set; } = null!;
        private static IContentPatcherAPI? ContentPatcherAPI { get; set; }

        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            ModHelper = helper;
            Manifest = ModManifest;
            
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        public override object GetApi()
        {
            return new CrossModCompatibilityToolsAPI();
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button is SButton.F1)
            {
                if (!Registrar.TryGetActionsFromEntry(Helper.ModRegistry.Get("Spiderbuttons.CMCTTest")!, out var actions, out var error))
                {
                    Log.Error(error);
                    return;
                }
                
                foreach (var action in actions)
                {
                    Log.Warn(action.Value.Id);
                }
            }
            
            if (e.Button is SButton.F2)
            {
                var api = new CrossModCompatibilityToolsAPI();
                if (!api.TryInvokeActionFromMod(Helper.ModRegistry.Get("Spiderbuttons.CMCTTest")!, "SecondTest", out var error))
                {
                    Log.Error(error);
                }
            }

            if (e.Button is SButton.F3)
            {
                var api = new CrossModCompatibilityToolsAPI();
                if (!api.TryInvokeActionFromMod(Helper.ModRegistry.Get("Spiderbuttons.CMCTTest")!, "TestAction", out var error))
                {
                    Log.Error(error);
                }
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            ContentPatcherAPI = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if (ContentPatcherAPI is not null)
            {
                ContentPatcherAPI.RegisterToken(Manifest, "Config", new ConfigToken());
                ContentPatcherAPI.RegisterToken(Manifest, "Translation", new TranslationToken());
                ContentPatcherAPI.RegisterToken(Manifest, "Dynamic", new DynamicToken());
                ContentPatcherAPI.RegisterToken(Manifest, "Asset", new InternalAssetToken());
                ContentPatcherAPI.RegisterToken(Manifest, "InternalAsset", new InternalAssetToken());
            }
        }
    }
}