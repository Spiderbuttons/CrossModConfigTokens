#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrossModConfigTokens.Helpers;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;

namespace CrossModConfigTokens
{
    /// <summary>Method delegates which represent a simplified version of <see cref="IValueProvider"/> that can be implemented by custom mod tokens through the API via <see cref="ConventionValueProvider"/>.</summary>
    /// <remarks>Methods should be kept in sync with <see cref="ConventionWrapper"/>.</remarks>
    internal class DynamicToken
    {
        /*********
         ** Fields
         *********/
        private object? dynamicTokenManager;

        private readonly object emptyInputArgs =
            Activator.CreateInstance(AccessTools.TypeByName("ContentPatcher.Framework.Tokens.EmptyInputArguments"),
                new object[] { })!;

        private string? uniqueID;
        private string dynamicTokenKey = string.Empty;
        private List<string> dynamicTokenValue = [];

        private bool refresh = false;

        /****
         ** Metadata
         ****/
        /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
        /// <remarks>Default false.</remarks>
        public bool AllowsInput()
        {
            return true;
        }

        /// <summary>Whether the token requires input arguments to work, and does not provide values without it (see <see cref="AllowsInput"/>).</summary>
        /// <remarks>Default false.</remarks>
        public bool RequiresInput()
        {
            return true;
        }

        /// <summary>Whether the token may return multiple values for the given input.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <remarks>Default true.</remarks>
        public bool CanHaveMultipleValues(string? input = null)
        {
            return false;
        }

        /// <summary>Validate that the provided input arguments are valid.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        /// <remarks>Default true.</remarks>
        public bool TryValidateInput(string? input, [NotNullWhen(false)] out string? error)
        {
            string[] split = input?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray() ??
                             [];
            if (split.Length != 2)
            {
                error = "Expected two arguments.";
                return false;
            }

            if (!ModEntry.ModList.ContainsKey(split[0]) && !ModEntry.PackList.ContainsKey(split[0]))
            {
                error = "Mod or pack not found.";
                return false;
            }

            error = null;
            return true;
        }

        /****
         ** State
         ****/
        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public bool UpdateContext()
        {
            return true;

            // if (dynamicTokenManager is null)
            // {
            //     dynamicTokenManager = ModEntry.GrabDynamicToken(uniqueID, dynamicTokenKey);
            // }
            //
            // if (dynamicTokenManager is null) return true;
            //
            // var oldValue = new List<string>(dynamicTokenValue);
            //
            // var values = AccessTools.Method(dynamicTokenManager.GetType(), "GetValues")
            //     .Invoke(dynamicTokenManager, new object[] { emptyInputArgs });
            // if (values is null)
            // {
            //     Log.Alert("No values! Returning true.");
            //     return true;
            // }
            //
            // dynamicTokenValue.Clear();
            // foreach (var value in (values as IEnumerable<string>)!)
            // {
            //     dynamicTokenValue.Add(value);
            // }
            //
            // var shouldUpdate = !oldValue.SequenceEqual(dynamicTokenValue);
            // Log.Alert($"Should update: {shouldUpdate}.");
            // return shouldUpdate;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public bool IsReady()
        {
            return ModEntry.ContentPatcherAPI != null && (ModEntry.ModList.Any() || ModEntry.PackList.Any()) &&
                   ModEntry.ContentPatcherAPI.IsConditionsApiReady;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if any.</param>
        public IEnumerable<string> GetValues(string? input)
        {
            if (input is null) yield break;
            var split = input?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray() ?? [];
            if (split.Length != 2)
            {
                yield break;
            }

            uniqueID = split[0];
            dynamicTokenKey = split[1];

            dynamicTokenManager = ModEntry.GrabDynamicToken(uniqueID, dynamicTokenKey);
            if (dynamicTokenManager is null) yield break;

            var values = AccessTools.Method(dynamicTokenManager!.GetType(), "GetValues")
                .Invoke(dynamicTokenManager, new object[] { emptyInputArgs });
            if (values is null) yield break;

            foreach (var value in (values as IEnumerable<string>)!)
            {
                yield return value;
            }
        }
    }
}