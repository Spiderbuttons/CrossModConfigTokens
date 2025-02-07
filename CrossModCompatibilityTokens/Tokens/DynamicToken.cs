using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrossModCompatibilityTokens.Readers;

namespace CrossModCompatibilityTokens.Tokens
{
    internal class DynamicToken
    {
        private readonly Dictionary<string, DynamicReader.DynamicTokenManager> DynamicCache = new();
        
        public DynamicToken()
        {
            foreach (var mod in ModEntry.ModHelper.ModRegistry.GetAll())
            {
                if (Registrar.TryGetContentPack(mod, out _, out _) && mod.Manifest.ContentPackFor?.UniqueID is "Pathoschild.ContentPatcher")
                {
                    DynamicCache[mod.Manifest.UniqueID] = new DynamicReader.DynamicTokenManager(mod.Manifest.UniqueID);
                }
            }
        }

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
            return true;
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
                error = "[Spiderbuttons.CMCT/Dynamic] Expected two input arguments (UniqueID and DynamicToken Name).";
                return false;
            }

            if (!Registrar.TryGetContentPack(split[0], out var _, out error))
            {
                error = $"[Spiderbuttons.CMCT/Dynamic] Content Patcher content pack '{split[0]}' not found.";
                return false;
            }

            if (!DynamicCache[split[0]].TryGetValues(split[1], out _, out error))
            {
                error = $"[Spiderbuttons.CMCT/Dynamic] DynamicToken '{split[1]}' not found in content pack '{split[0]}'.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public bool UpdateContext()
        {
            return true;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public bool IsReady()
        {
            return Registrar.AreAllModsLoaded();
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if any.</param>
        public IEnumerable<string> GetValues(string? input)
        {
            if (input is null) yield break;
            var split = input.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (split.Length != 2)
            {
                yield break;
            }

            var uniqueId = split[0];
            var name = split[1];

            // Still can't figure out how to do this with a cache. The token is always late by a day if I don't grab it uncached...
            if (DynamicCache.TryGetValue(uniqueId, out var manager) && manager.TryGetValuesNoCache(name, out var values, out _))
            {
                foreach (var value in values)
                {
                    yield return value;
                }
            }
        }
    }
}