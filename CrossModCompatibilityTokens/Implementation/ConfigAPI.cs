using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CrossModCompatibilityTokens.API;
using CrossModCompatibilityTokens.Helpers;
using CrossModCompatibilityTokens.Readers;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.Implementation;

public partial class CrossModCompatibilityToolsAPI : ICrossModCompatibilityToolsAPI
{
    public bool TryGetConfigValue(IModInfo mod, string configKey, [NotNullWhen(true)] out string? configValue, out string? error)
    {
        configValue = null;
        error = null;
        if (!ConfigReader.TryGetModConfigValue(mod.Manifest.UniqueID, configKey, out configValue, out error))
        {
            return false;
        }
        return true;
    }

    public bool TryGetConfigValue<T>(IModInfo mod, string configKey, [NotNullWhen(true)] out T? configValue, out string? error)
    {
        configValue = default;
        error = null;
        if (!ConfigReader.TryGetModConfigValue<T>(mod.Manifest.UniqueID, configKey, out configValue, out error))
        {
            return false;
        }
        return true;
    }
    
    public bool TryGetConfig(IModInfo mod, [NotNullWhen(true)] out Dictionary<string, object>? configObject, out string? error)
    {
        configObject = null;
        error = null;
        if (!ConfigReader.TryGetModConfig(mod.Manifest.UniqueID, out var config, out error))
        {
            return false;
        }
        
        configObject = config.ToObject<Dictionary<string, object>>();
        if (configObject == null)
        {
            error = $"Failed to parse config from mod with UniqueID '{mod.Manifest.UniqueID}' as Dictionary<string, object>";
            return false;
        }
        
        return true;
    }
    
    public bool TryGetConfig(IModInfo mod, [NotNullWhen(true)] out JObject? configObject, out string? error)
    {
        configObject = null;
        error = null;
        if (!ConfigReader.TryGetModConfig(mod.Manifest.UniqueID, out configObject, out error))
        {
            return false;
        }
        return true;
    }

    public string? GetConfigValue(IModInfo mod, string configKey)
    {
        if (!TryGetConfigValue(mod, configKey, out var value, out var error))
        {
            Log.Error(error);
            return null;
        }
        return value;
    }

    public T? GetConfigValue<T>(IModInfo mod, string configKey)
    {
        if (!TryGetConfigValue(mod, configKey, out T? value, out var error))
        {
            Log.Error(error);
            return default;
        }
        return value;
    }

    public Dictionary<string, object>? GetConfig(IModInfo mod)
    {
        if (!TryGetConfig(mod, out Dictionary<string,object>? config, out var error))
        {
            Log.Error(error);
            return null;
        }
        return config;
    }

    public JObject? GetConfigJObject(IModInfo mod)
    {
        if (!TryGetConfig(mod, out JObject? config, out var error))
        {
            Log.Error(error);
            return null;
        }
        return config;
    }
}