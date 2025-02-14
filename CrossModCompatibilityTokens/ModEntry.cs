using CrossModCompatibilityTokens.Implementation;
using CrossModCompatibilityTokens.Integration;
using CrossModCompatibilityTokens.Readers;
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
            if (e.Button is SButton.F2)
            {
                //
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            TranslationReader.BuildCache();
            DynamicReader.BuildCache();
            
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