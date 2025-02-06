#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrossModCompatibilityTokens.Helpers;
using CrossModCompatibilityTokens.Readers;
using Newtonsoft.Json.Linq;

namespace CrossModCompatibilityTokens.Tokens
{
    internal class ConfigToken
    {
        private readonly Dictionary<string, ConfigReader.ModConfigManager> ConfigCache = new();
        
        public ConfigToken()
        {
            foreach (var mod in ModEntry.ModHelper.ModRegistry.GetAll())
            {
                if (ConfigReader.TryGetModConfig(mod, out _, out _))
                {
                    ConfigCache[mod.Manifest.UniqueID] = new ConfigReader.ModConfigManager(mod.Manifest.UniqueID);
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
            string[] split = input?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray() ??
                             [];
            if (split.Length != 2)
            {
                error = "[Spiderbuttons.CMCT/Config] Expected two input arguments (UniqueID and Config Name).";
                return false;
            }

            if (!Registrar.TryGetModMetadata(split[0], out var _, out error))
            {
                error = $"[Spiderbuttons.CMCT/Config] Mod or Content Pack '{split[0]}' not found.";
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
            foreach (var (_, config) in ConfigCache)
            {
                foreach (var cfgKey in config.GetCachedKeys())
                {
                    if (config.TryGetConfig<string>(cfgKey, out var oldValue, out _) && config.TryGetConfigNoCache<string>(cfgKey, out var newValue, out _))
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
            var configKey = split[1];

            if (ConfigCache.TryGetValue(uniqueId, out var modConfig) && modConfig.TryGetConfig<string>(configKey, out var config, out var error))
            {
                yield return config;
            }
        }
    }
}