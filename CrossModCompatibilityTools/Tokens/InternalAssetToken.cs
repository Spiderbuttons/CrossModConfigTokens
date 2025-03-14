using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrossModCompatibilityTools.Helpers;
using CrossModCompatibilityTools.Readers;
using StardewModdingAPI;

namespace CrossModCompatibilityTools.Tokens
{
    internal class InternalAssetToken
    {
        private readonly Dictionary<string, InternalAssetReader.InternalAssetManager> AssetCache = new();
        
        public InternalAssetToken()
        {
            foreach (var mod in ModEntry.ModHelper.ModRegistry.GetAll())
            {
                if (InternalAssetReader.TryGetModContent(mod, out _, out _))
                {
                    AssetCache[mod.Manifest.UniqueID] = new InternalAssetReader.InternalAssetManager(mod.Manifest.UniqueID);
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
                error = "[Spiderbuttons.CMCT/InternalAsset] Expected two input arguments (UniqueID and Asset Path).";
                ModEntry.ModMonitor.LogOnce(error, LogLevel.Warn);
                return false;
            }

            if (!ModList.TryGetModMetadata(split[0], out _, out error))
            {
                error = $"[Spiderbuttons.CMCT/InternalAsset] Mod or Content Pack '{split[0]}' not found.";
                ModEntry.ModMonitor.LogOnce(error, LogLevel.Warn);
                return false;
            }
            
            if (!AssetCache[split[0]].TryGetValue(split[1], out _, out error))
            {
                error = $"[Spiderbuttons.CMCT/InternalAsset] Asset with path '{split[1]}' not found in mod or content pack '{split[0]}'.";
                ModEntry.ModMonitor.LogOnce(error, LogLevel.Warn);
                return false;
            }

            error = null;
            return true;
        }
        
        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public bool UpdateContext()
        {
            bool shouldUpdate = false;
            foreach (var (_, content) in AssetCache)
            {
                foreach (var path in content.GetCachedPaths())
                {
                    if (content.TryGetValue(path, out var oldValue, out _) && content.TryGetValueNoCache(path, out var newValue, out _))
                    {
                        if (oldValue != newValue) shouldUpdate = true;
                    }
                }
            }

            return shouldUpdate;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public bool IsReady()
        {
            return ModList.AreAllModsLoaded();
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
            var path = split[1];

            string? error = null;
            if (AssetCache.TryGetValue(uniqueId, out var manager) && manager.TryGetValue(path, out var asset, out error))
            {
                yield return asset.BaseName;
            } else Log.Warn($"Unable to retrieve internal asset with path '{path}' from mod '{uniqueId}': {error}");
        }
    }
}