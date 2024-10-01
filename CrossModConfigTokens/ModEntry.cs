#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using ContentPatcher;
using ContentPatcher.Framework;
using CrossModConfigTokens.Helpers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Object = StardewValley.Object;

namespace CrossModConfigTokens
{
    internal sealed class ModEntry : Mod
    {
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static IMonitor ModMonitor { get; set; } = null!;
        
        internal static IManifest Manifest { get; set; } = null!;

        internal static object ModRegistry { get; set; } = null!;

        internal static Dictionary<string, IMod> ModList { get; set; } = [];

        internal static Dictionary<string, IContentPack> PackList { get; set; } = [];

        internal static IContentPatcherAPI? ContentPatcherAPI { get; set; } = null;

        internal static object? TokenManager { get; set; } = null!;
        internal static object? LocalTokens { get; set; } = null!;
        
        internal static bool TokensReady { get; set; } = false;

        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            ModMonitor = Monitor;
            Manifest = ModManifest;

            var SCore = typeof(Mod).Assembly.GetType("StardewModdingAPI.Framework.SCore")!.GetProperty("Instance",
                BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null);
            ModRegistry = AccessTools.Field(SCore!.GetType(), "ModRegistry")?.GetValue(SCore)!;


            Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
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
        }
        
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (ContentPatcherAPI is not { IsConditionsApiReady: true }) return;
            GrabTokenManager();
            TokensReady = true;
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
            Log.Alert("Found token manager!");
        }

        public static object? GrabDynamicToken(string? uniqueID, string tokenKey)
        {
            if (LocalTokens is null || uniqueID is null) return null;
            try
            {
                var modCachedContext = AccessTools.Property(LocalTokens!.GetType(), "Item")
                    ?.GetValue(LocalTokens, new object[] { uniqueID });
                var modContext = AccessTools.Property(modCachedContext!.GetType(), "Context")
                    ?.GetValue(modCachedContext);
                
                // cre
                Log.Warn($"Grabbing token '{tokenKey}' from '{uniqueID}'");
                return modContext!.GetType().GetMethod("GetToken")?.Invoke(modContext, new object[] { tokenKey, true });
            } 
            catch (Exception e)
            {
                Log.Error($"Error grabbing dynamic token: {e}");
            }
            
            return null;
        }

        public static bool ShouldUpdateDynamicToken(string? uniqueID, string token)
        {
            if (LocalTokens is null || uniqueID is null) return false;
            
            Log.Alert("Got here super first");
            
            var modCachedContext = LocalTokens.GetType().GetProperty("Item")?.GetValue(LocalTokens, new object[] { uniqueID });
            if (modCachedContext is null) return false;
            
            var modTokenContext = AccessTools.Property(modCachedContext!.GetType(), "Context")?.GetValue(modCachedContext);
            if (modTokenContext is null) return false;

            var tokenObj = modTokenContext.GetType().GetMethod("GetToken")?.Invoke(modTokenContext, new object[] { token, true });
            if (tokenObj is null) return false;
            Log.Error("Got tokenObj");

            var shouldUpdate = tokenObj.GetType().GetMethod("UpdateContext")
                ?.Invoke(tokenObj, new object[] { modTokenContext });
            Log.Warn($"Should update: {shouldUpdate}");
            
            var dynList = AccessTools.Field(modTokenContext!.GetType(), "DynamicTokenValues")?.GetValue(modTokenContext) as object;
            
            Log.Alert("Got dynList");
            
            var newValue = string.Empty;
            
            foreach (var dyn in (IEnumerable<object>)dynList!)
            {
                var key = dyn.GetType().GetProperty("Name")?.GetValue(dyn);
                if (key is null || !key.ToString()!.Equals(token)) continue;

                var parentToken = dyn.GetType().GetProperty("ParentToken")?.GetValue(dyn);
                if (parentToken is null) continue;
                Log.Alert("Got parentToken");
                var valueProvider = parentToken!.GetType().GetProperty("ValueProvider")?.GetValue(parentToken);
                if (valueProvider is null) continue;
                Log.Alert("Got valueProvider");
                
                return valueProvider!.GetType().GetProperty("IsMutable")?.GetValue(valueProvider) as bool? ?? false;

                // valueProvider!.GetType().GetMethod("UpdateContext")?.Invoke(valueProvider, new object[] { TokenManager });
                // var values = valueProvider!.GetType().GetMethod("GetValues")?.Invoke(valueProvider, new object[] { input }) as IEnumerable<string>;
                // if (values is null) continue;
                // Log.Alert("Got values");
                // foreach (var value in values)
                // {
                //     if (newValue == string.Empty)
                //     {
                //         newValue = value;
                //     }
                //     else
                //     {
                //         newValue += $", {value}";
                //     }
                // }

                break;

                // var conditions = dyn.GetType().GetProperty("Conditions")?.GetValue(dyn) as object[];
                // var shouldSkip = true;
                // foreach (var condition in conditions!)
                // {
                //     var doesApply = condition.GetType().GetMethod("UpdateContext")?.Invoke(condition, new object[] { context }) as bool? ?? false;
                //     if (!doesApply)
                //     {
                //         shouldSkip = false;
                //     }
                // }
                //
                // if (shouldSkip)
                // {
                //     Log.Info($"Skipping token {key} due to conditions.");
                //     continue;
                // }
                //
                // Log.Warn($"Updating value from {currentValue} to {dyn.GetType().GetProperty("Value")?.GetValue(dyn)}");
                // currentValue = dyn.GetType().GetProperty("Value")?.GetValue(dyn)?.ToString();
            }

            return false;
        }

        private static Dictionary<string, string> GrabReadyDynamicTokensFromMod(string uniqueID)
        {
            if (LocalTokens is null) return new Dictionary<string, string>();
            
            var modCachedContext = LocalTokens.GetType().GetProperty("Item")?.GetValue(LocalTokens, new object[] { uniqueID });
            var context = AccessTools.Property(modCachedContext!.GetType(), "Context")?.GetValue(modCachedContext);
            var dynList = AccessTools.Field(context!.GetType(), "DynamicTokenValues")?.GetValue(context) as object;
            
            var tokenDict = new Dictionary<string, string>();
            foreach (var dyn in (IEnumerable<object>)dynList!)
            {
                // call the UpdateContext method first, pass in context as the parameter
                var shouldUpdate = dyn.GetType().GetMethod("UpdateContext")?.Invoke(dyn, new object[] { context }) as bool? ?? false;
                
                var isReady = dyn.GetType().GetProperty("IsReady")?.GetValue(dyn) as bool? ?? false;
                //if (!isReady) continue;
                
                var key = dyn.GetType().GetProperty("Name")?.GetValue(dyn);
                var value = dyn.GetType().GetProperty("Value")?.GetValue(dyn);
                
                if (key is null || value is null) continue;
                
                var conditions = dyn.GetType().GetProperty("Conditions")?.GetValue(dyn) as object[];
                var shouldSkip = false;
                if (conditions is not null)
                {
                    foreach (var condition in conditions)
                    {
                        var doesApply = condition.GetType().GetProperty("IsMatch")?.GetValue(condition) as bool? ?? false;
                        if (!doesApply)
                        {
                            shouldSkip = true;
                            break;
                        }
                    }
                }

                if (shouldSkip) continue;
                tokenDict[key.ToString()!] = value.ToString()!;
            }
            
            return tokenDict;
        }
        
        public static string? GrabDynamicTokenValue(string uniqueID, string? tokenName)
        {
            if (uniqueID.Equals(Manifest.UniqueID)) return null;
            if (tokenName is null)
            {
                Log.Trace("Tried to grab a dynamic token without a token name! This may happen at startup regardless, but double check your token names just in case.");
                return null;
            }
            var tokenDict = GrabReadyDynamicTokensFromMod(uniqueID);
            if (tokenDict.TryGetValue(tokenName, out var value))
            {
                return value;
            }

            Log.Warn($"Token '{tokenName}' not found in {uniqueID}!");
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

        public static string GrabTranslationString(string uniqueID, string key)
        {
            var translations = GrabTranslations(uniqueID);
            if (translations == null) return string.Empty;

            foreach (var translation in translations)
            {
                Log.Alert(translation);
            }

            return string.Empty;
        }

        private static IEnumerable<Translation>? GrabTranslations(string uniqueID)
        {
            try
            {
                if (ModList.TryGetValue(uniqueID, out var mod))
                {
                    return mod.Helper.Translation.GetTranslations();
                }

                if (PackList.TryGetValue(uniqueID, out var pack))
                {
                    return pack.Translation.GetTranslations();
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error grabbing translations for {uniqueID}: {e}");
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

            return null!;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // if (!Context.IsWorldReady)
            //     return;

            if (e.Button is SButton.F5)
            {
                Log.Alert(GrabDynamicTokenValue("Test.Mod", "TestDynToken"));
                Log.Alert(GrabConfigValue("Spiderbuttons.ButtonsExtraBooksCore", "CheatCodesPrice").ToString());

                // var cpType = typeof(ContentPatcherAPI).Assembly.GetType("ContentPatcher.ModEntry");
                // var cpMod = ModList["Pathoschild.ContentPatcher"];
                // // grab the ContentPacks field from cpMod
                // var sManager = AccessTools.Field(cpType, "ScreenManager")?.GetValue(cpMod);
                // Log.Warn($"Got manager: {sManager}");
                // var screen = AccessTools.Property(sManager.GetType(), "Value")?.GetValue(sManager);
                // Log.Warn($"Got screen: {screen}");
                // var tokenManager =
                //     AccessTools.Property(screen.GetType(), "TokenManager")?.GetValue(screen) as object;
                // Log.Warn("Got tokens");
                // var localTokens =
                //     AccessTools.Field(tokenManager.GetType(), "LocalTokens")?.GetValue(tokenManager) as object;
                // Log.Warn($"Got local tokens: {localTokens}");
                // // localTokens is subclassed as a dictionary, so invoke the direct access operator with the "Test.Mod" key
                // var testMod = localTokens.GetType().GetProperty("Item")?.GetValue(localTokens, new object[] { "Test.Mod" });
                // Log.Warn($"Got test mod: {testMod}");
                // var context = AccessTools.Property(testMod.GetType(), "Context")?.GetValue(testMod);
                // Log.Warn($"Got context: {context}");
                // var dynList = AccessTools.Field(context.GetType(), "DynamicTokenValues")?.GetValue(context) as object;
                // Log.Warn($"Got dynamic list: {dynList}");
                // foreach (var dyn in (IEnumerable<object>)dynList)
                // {
                //     Log.Warn($"Got dynamic: {dyn}");
                //     var key = dyn.GetType().GetProperty("Name")?.GetValue(dyn);
                //     var value = dyn.GetType().GetProperty("Value")?.GetValue(dyn);
                //     Log.Warn($"Key: {key}, Value: {value}");
                // }


                // localTokens is subclassed from dictionary, so invoke the GetEnumerator method
                // var enumer = localTokens.GetType().GetMethod("GetEnumerator")?.Invoke(localTokens, new object[] { });
                // // enumerate the tokens
                // while (enumer is not null && (bool)enumer.GetType().GetMethod("MoveNext")?.Invoke(enumer, new object[] { }))
                // {
                //     var current = enumer.GetType().GetProperty("Current")?.GetValue(enumer);
                //     var key = current.GetType().GetProperty("Key")?.GetValue(current);
                //     var value = current.GetType().GetProperty("Value")?.GetValue(current);
                //     Log.Warn($"Key: {key}, Value: {value}");
                // }
            }
        }
    }
}