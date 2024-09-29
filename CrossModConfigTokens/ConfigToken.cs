#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrossModConfigTokens.Helpers;
using Newtonsoft.Json.Linq;

namespace CrossModConfigTokens
{
    /// <summary>Method delegates which represent a simplified version of <see cref="IValueProvider"/> that can be implemented by custom mod tokens through the API via <see cref="ConventionValueProvider"/>.</summary>
    /// <remarks>Methods should be kept in sync with <see cref="ConventionWrapper"/>.</remarks>
    internal class ConfigToken
    {
        /*********
         ** Fields
         *********/
        private string uniqueID = "Spiderbuttons.CMCT";
        private string? configValue;

        /****
         ** Metadata
         ****/
        /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
        /// <remarks>Default false.</remarks>
        public bool AllowsInput()
        {
            return false;
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
            string[] split = input?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray() ?? [];
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
            var oldConfig = configValue;
            var newConfigValue = ModEntry.GrabConfigValue(uniqueID, configValue);
            if (newConfigValue == null) return oldConfig != null;

            configValue = newConfigValue.Value<string>();
            return oldConfig != configValue;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public bool IsReady()
        {
            return ModEntry.ModList.Any() || ModEntry.PackList.Any();
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
            configValue = ModEntry.GrabConfigValue(uniqueID, split[1])?.Value<string>();
            
            yield return configValue ?? string.Empty;
        }
    }
}