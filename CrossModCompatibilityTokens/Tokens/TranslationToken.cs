#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrossModCompatibilityTokens.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StardewModdingAPI;
using StardewValley;

namespace CrossModCompatibilityTokens.Tokens
{
    /// <summary>Method delegates which represent a simplified version of <see cref="IValueProvider"/> that can be implemented by custom mod tokens through the API via <see cref="ConventionValueProvider"/>.</summary>
    /// <remarks>Methods should be kept in sync with <see cref="ConventionWrapper"/>.</remarks>
    internal class TranslationToken
    {
        /*********
         ** Fields
         *********/
        private Dictionary<string, ITranslationHelper> TransCache = new();
        private ITranslationHelper? TranslationHelper;
        private LocalizedContentManager.LanguageCode LastLocale;

        public TranslationToken()
        {
            foreach (var mod in ModEntry.ModHelper.ModRegistry.GetAll())
            {
                if (TranslationReader.TryGetModTranslator(mod, out var translator, out var error))
                {
                    TransCache[mod.Manifest.UniqueID] = translator;
                }
            }
        }

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
            string[] split = input?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray() ?? [];
            if (split.Length != 2)
            {
                error = "Expected two arguments.";
                return false;
            }
            
            if (!Registrar.TryGetModMetadata(split[0], out var _, out error))
            {
                error = $"Mod or Content Pack '{split[0]}' not found.";
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
            if (LastLocale == ModEntry.ModHelper.Translation.LocaleEnum)
            {
                return false;
            }
            
            LastLocale = ModEntry.ModHelper.Translation.LocaleEnum;
            return true;
            
            // if (this.TranslationHelper is null) return true;
            // if (this.TranslationHelper.LocaleEnum == this.LastLocale) return false;
            //
            // this.LastLocale = this.TranslationHelper.LocaleEnum;
            // return true;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public bool IsReady()
        {
            return true;
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

            var uniqueID = split[0];
            if (TransCache.TryGetValue(uniqueID, out var translator))
            {
                yield return translator.Get(split[1]);
            }
        }
    }
}