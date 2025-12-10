using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CrossModCompatibilityTools.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using CrossModCompatibilityTools.Implementation;
using CrossModCompatibilityTools.Integration;
using CrossModCompatibilityTools.Readers;
using CrossModCompatibilityTools.Tokens;
using MonoMod.Utils;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Internal.ConsoleWriting;
using StardewModdingAPI.Utilities;
using StardewValley.Extensions;

namespace CrossModCompatibilityTools
{
    internal sealed class ModEntry : Mod
    {
        private const string RESET = "\x1B[22m\x1B[23m";
        private const string LIGHT = "\x1B[2m";
        
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

            Helper.ConsoleCommands.Add("cmct_actions",
                "List actions registered with Cross-Mod Compatibility Tools.\n\nUsage: cmct actions [uniqueID]\nUsage: cmct actions [detailed]\nUsage: cmct actions [uniqueID] [detailed]\n- uniqueID: the uniqueID of a mod to view the registered actions for\n- detailed: whether to show CustomData information for each action if it exists\n\nExample: cmct actions Spiderbuttons.ProducerExample detailed",
                this.LogActions);
        }

        public override object GetApi(IModInfo modInfo)
        {
            return new CrossModCompatibilityToolsAPI(modInfo);
        }

        private void LogActions(string command, string[] args)
        {
            if (!Registrar.TryGetAllActions(out var allActions, out var error))
            {
                Log.Error($"Error: {error}");
                return;
            }

            var actions = allActions.OrderBy(a => a.Mod.Manifest.UniqueID).ToList();
            var logString = $"\n";
            
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions.ElementAt(i);
                if (args.Length > 0 && action.Mod.Manifest.UniqueID != args[0] && !args[0].EqualsIgnoreCase("detailed"))
                {
                    continue;
                }
                logString += $"Action ID: {LIGHT}{action.Id}{RESET}\nRegistrant: {LIGHT}{action.Mod.Manifest.Name}{RESET}\nName: {LIGHT}{action.Name()}{RESET}\nDescription: {LIGHT}{action.Description()}{RESET}";
                
                if ((args.Length > 0 && args[0].EqualsIgnoreCase("detailed") ||
                     args.Length > 1 && args[1].EqualsIgnoreCase("detailed")) &&
                    action.RawCustomData is not null)
                {
                    logString += $"\nCustomData: {{\n";
                    // reflect to get the member info of the data and log the name of each field/property/method/etc
                    foreach (var member in action.RawCustomData.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    {
                        if (member.IsDefined(typeof(CompilerGeneratedAttribute), false)) continue;
                        if (member.MemberType is MemberTypes.Constructor) continue;
                        logString += "    ";
                        if (member is FieldInfo field)
                        {
                            logString += $"{LIGHT}Field    |{RESET} {field.FieldType} {field.Name}";
                        } else if (member is PropertyInfo prop)
                        {
                            logString += $"{LIGHT}Property |{RESET} {prop.PropertyType} {prop.Name}";
                        } else if (member is MethodInfo method)
                        {
                            logString += $"{LIGHT}Method   |{RESET} {method.ReturnType} {method.Name}{(method.GetParameters().Length > 0 ? $"({string.Join(", ", method.GetParameters().Select(p => p.ParameterType))})" : "()")}";
                        }
                        logString += $"\n";
                    }

                    logString += $"}}";
                }
                if (i < actions.Count - 1)
                {
                    logString += $"\n\n";
                }
            }
            
            Log.Info(logString);
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