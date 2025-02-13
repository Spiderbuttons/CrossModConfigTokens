using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CrossModCompatibilityTokens.API;
using CrossModCompatibilityTokens.Helpers;
using CrossModCompatibilityTokens.Readers;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace CrossModCompatibilityTokens.Implementation;

public partial class CrossModCompatibilityToolsAPI : ICrossModCompatibilityToolsAPI
{
    private static Dictionary<string, object> ConfigClassCache { get; } = new();
    
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
    
    public bool TryGetConfigJObject(IModInfo mod, [NotNullWhen(true)] out JObject? configObject, out string? error)
    {
        configObject = null;
        error = null;
        if (!ConfigReader.TryGetModConfig(mod.Manifest.UniqueID, out configObject, out error))
        {
            return false;
        }
        return true;
    }

    public bool TryGetConfigClass(IModInfo mod, [NotNullWhen(true)] out object? configClass, out string? error)
    {
        error = null;
        configClass = null;
        if (!ModList.TryGetMod(mod, out var modInstance, out error))
        {
            return false;
        }
        
        if (!TryGetConfig(mod, out var config, out error))
        {
            return false;
        }
        
        if (ConfigClassCache.TryGetValue(mod.Manifest.UniqueID, out var cachedConfigClass))
        {
            configClass = cachedConfigClass;
            return true;
        }
        
        foreach (var member in modInstance.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            if (member is not (PropertyInfo or FieldInfo) || !TryCompareConfigKeys(member, config.Keys.ToList(), out configClass)) continue;
            ConfigClassCache[mod.Manifest.UniqueID] = configClass;
            return true;
        }
        
        error = $"Failed to find config class for mod with UniqueID '{mod.Manifest.UniqueID}'";
        return false;
    }

    private bool TryCompareConfigKeys(MemberInfo member, List<string> configKeys, [NotNullWhen(true)] out object? configClass)
    {
        configClass = null;
        switch (member)
        {
            case PropertyInfo { PropertyType: not { IsClass: true, IsAbstract: false, IsGenericType: false } }:
            case FieldInfo { FieldType: not { IsClass: true, IsAbstract: false, IsGenericType: false } }:
                return false;
        }
        
        var memberValue = member switch
        {
            PropertyInfo property => property.GetValue(property),
            FieldInfo field => field.GetValue(field),
            _ => null
        };
        if (memberValue == null) return false;
        
        var memberValueProperties = memberValue.GetType().GetProperties();
        var memberValueFields = memberValue.GetType().GetFields();
        if (!configKeys.All(key => memberValueProperties.Any(p => p.Name.EqualsIgnoreCase(key)) || memberValueFields.Any(f => f.Name.EqualsIgnoreCase(key))))
        {
            return false;
        }
        
        configClass = memberValue;
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
        if (!TryGetConfigJObject(mod, out JObject? config, out var error))
        {
            Log.Error(error);
            return null;
        }
        return config;
    }
}