using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace CrossModCompatibilityTools.Readers;

public static class ConfigReader
{
    public static Dictionary<Type, MethodInfo> ParseMethodCache { get; } = new();
    public static Dictionary<Type, MethodInfo> TryParseMethodCache { get; } = new();
    
    public class ModConfigManager(string uniqueId)
    {
        private string ModId { get; } = uniqueId;
        private Dictionary<string, object> Cache { get; } = new();

        public bool TryGetConfig<T>(string key, [NotNullWhen(true)] out T? config, out string? error)
        {
            error = null;
            config = default;
            if (!Cache.TryGetValue(key, out var value)) 
                return TryGetConfigNoCache(key, out config, out error);
            
            config = (T)value;
            return true;

        }

        public bool TryGetConfigNoCache<T>(string key, [NotNullWhen(true)] out T? config, out string? error)
        {
            error = null;
            config = default;
            if (!TryGetModConfigValue(ModId, key, out T? value, out error))
                return false;
            
            Cache[key] = value;
            config = value;
            return true;
        }
        
        public IEnumerable<string> GetCachedKeys()
        {
            return Cache.Keys;
        }
    }
    
    public static bool TryGetModConfig(string uniqueId, [NotNullWhen(true)] out JObject? config, [NotNullWhen(false)] out string? error)
    {
        config = null;
        error = null;
        if (!ModList.TryGetModMetadata(uniqueId, out var metadata, out error))
            return false;

        try
        {
            if (metadata.IsContentPack)
            {
                if (ModList.TryGetContentPack(metadata.Manifest.UniqueID, out var pack, out error))
                {
                    if (pack.HasFile("config.json"))
                    {
                        config = pack.ReadJsonFile<JObject>("config.json")!;
                        return true;
                    }

                    error = $"Content pack '{metadata.Manifest.UniqueID}' does not have a config!";
                    return false;
                }
            }
            else
            {
                if (ModList.TryGetMod(uniqueId, out var mod, out error))
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
    
    public static bool TryGetModConfig(IModInfo mod, [NotNullWhen(true)] out JObject? config, [NotNullWhen(false)] out string? error)
    {
        return TryGetModConfig(mod.Manifest.UniqueID, out config, out error);
    }
    
    public static bool TryGetModConfigValue<T>(string uniqueId, string key, [NotNullWhen(true)] out T? value, [NotNullWhen(false)] out string? error)
    {
        value = default;
        error = null;
        if (!TryGetModConfig(uniqueId, out var config, out error))
            return false;

        var keySplit = key.Split('.');
        var currentValue = config.GetValue(keySplit[0]);
        if (keySplit.Length == 1) goto tryParse;
        
        for (var i = 1; i < keySplit.Length; i++)
        {
            if (currentValue is not JObject currentObject)
            {
                error = $"Config schema from '{uniqueId}' does not have a config matching '{key}'!";
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
            bool foundInCache;
            if ((foundInCache = ParseMethodCache.ContainsKey(typeof(T))) || typeof(T).GetMethod("Parse", [typeof(string)]) is not null)
            {
                if (!foundInCache) ParseMethodCache.TryAdd(typeof(T), typeof(T).GetMethod("Parse", [typeof(string)])!);
                var result = ParseMethodCache[typeof(T)].Invoke(null, [currentValue.ToString()]);
                if (result is null or false)
                {
                    error = $"Failed to parse value of key '{key}' in {uniqueId}'s config as type {typeof(T)}.";
                    return false;
                }
            
                value = (T)result;
                return true;
            }
            
            if ((foundInCache = TryParseMethodCache.ContainsKey(typeof(T))) || typeof(T).GetMethod("TryParse", [typeof(string), typeof(T).MakeByRefType()]) is not null)
            {
                if (!foundInCache) TryParseMethodCache.TryAdd(typeof(T), typeof(T).GetMethod("TryParse", [typeof(string), typeof(T).MakeByRefType()])!);
                object?[] args = [currentValue.ToString(), null];
                var result = TryParseMethodCache[typeof(T)].Invoke(null, args);
                if (result is null or false)
                {
                    error = $"Failed to parse value of key '{key}' in {uniqueId}'s config as type {typeof(T)}.";
                    return false;
                }
            
                value = (T)args[1]!;
                return true;
            }
            value = currentValue.ToObject<T>()!;
            return true;
        }
        catch (Exception e)
        {
            error = $"Error casting value of key '{key}' in {uniqueId}'s config to type {typeof(T)}: {e}";
            return false;
        }
    }
}