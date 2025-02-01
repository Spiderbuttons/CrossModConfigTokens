using System;
using CrossModCompatibilityTokens.Helpers;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Framework;
using StardewValley.Extensions;

namespace CrossModCompatibilityTokens;

public static class ConfigReader
{
    public static bool TryGetConfig(string uniqueId, out JObject? config, out string? error)
    {
        config = null;
        error = null;
        if (!Registrar.TryGetModMetadata(uniqueId, out var metadata, out error))
        {
            return false;
        }

        try
        {
            if (metadata.IsContentPack)
            {
                if (Registrar.TryGetContentPack(metadata.Manifest.UniqueID, out var pack, out error))
                {
                    config = pack.ReadJsonFile<JObject>("config.json");
                    return true;
                }
            }
            else
            {
                if (Registrar.TryGetMod(uniqueId, out var mod, out error))
                {
                    config = mod.Helper.ModContent.Load<JObject>("config.json");
                    return true;
                }
            }
        }
        catch (Exception)
        {
            error = $"Mod with UniqueID '{uniqueId}' does not have a config!";
            return false;
        }

        return false;
    }
    
    public static bool TryGetConfigValue<T>(string uniqueId, string key, out T? value, out string? error)
    {
        value = default;
        error = null;
        if (!TryGetConfig(uniqueId, out var config, out error))
        {
            return false;
        }

        if (config is null)
        {
            error = $"Mod with UniqueID '{uniqueId}' does not have a config!";
            return false;
        }

        var keySplit = key.Split('.');
        var currentValue = config.GetValue(keySplit[0]);
        if (keySplit.Length == 1) goto tryParse;
        
        for (var i = 1; i < keySplit.Length; i++)
        {
            if (currentValue is not JObject currentObject)
            {
                Log.Warn($"Config schema from '{uniqueId}' does not have a config matching '{key}'!");
                return false;
            }
            currentValue = currentObject.GetValue(keySplit[i]);
        }

        tryParse:
        if (currentValue is null)
        {
            error = $"Config schema from '{uniqueId}' does not have a config matching '{key}'!";
            return false;
        }
        
        try
        {
            if (typeof(T).GetMethod("Parse") is { } parseMethod && parseMethod.GetParameters()[0].ParameterType.Name.EqualsIgnoreCase("string"))
            {
                var result = parseMethod.Invoke(null, new object?[] { currentValue.ToString() });
                if (result is null or false)
                {
                    error = $"Failed to parse value of key '{key}' in {uniqueId}'s config as type {typeof(T)}.";
                    return false;
                }
            
                value = (T)result;
                return true;
            }
            
            if (typeof(T).GetMethod("TryParse") is { } tryParseMethod && tryParseMethod.GetParameters()[0].ParameterType.Name.EqualsIgnoreCase("string"))
            {
                object?[] args = { currentValue.ToString(), default, default };
                var result = tryParseMethod.Invoke(null, args);
                if (result is null or false)
                {
                    error = $"Failed to parse value of key '{key}' in {uniqueId}'s config as type {typeof(T)}.";
                    return false;
                }
            
                value = (T)args[1]!;
                return true;
            }
            value = currentValue.ToObject<T>();
            return true;
        }
        catch (Exception e)
        {
            error = $"Error casting value of key '{key}' in {uniqueId}'s config to type {typeof(T)}: {e}";
            return false;
        }
    }
}