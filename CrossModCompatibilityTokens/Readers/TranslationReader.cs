using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CrossModCompatibilityTokens.Helpers;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.Readers;

public static class TranslationReader
{
    public static readonly Dictionary<string, ITranslationHelper> TransCache = new();

    public static void BuildCache()
    {
        foreach (var mod in ModEntry.ModHelper.ModRegistry.GetAll())
        {
            if (TryGetModTranslationHelper(mod, out var translator, out _))
            {
                TransCache[mod.Manifest.UniqueID] = translator;
            }
        }
    }
    
    public static bool TryGetModTranslationHelper(string uniqueId, [NotNullWhen(true)] out ITranslationHelper? translator, out string? error)
    {
        translator = null;
        error = null;
        
        if (TransCache.TryGetValue(uniqueId, out translator))
        {
            Log.Alert("Found from cache");
            return true;
        }
        
        if (!ModList.TryGetModMetadata(uniqueId, out var metadata, out error))
        {
            return false;
        }

        translator = metadata.Translations!;
        return true;
    }
    
    public static bool TryGetModTranslationHelper(IModInfo mod, [NotNullWhen(true)] out ITranslationHelper? translator, out string? error)
    {
        return TryGetModTranslationHelper(mod.Manifest.UniqueID, out translator, out error);
    }
    
    public static bool TryGetModTranslation(string uniqueId, string key, object? tokens, [NotNullWhen(true)] out Translation? value, out string? error)
    {
        value = null;
        error = null;
        if (!TryGetModTranslationHelper(uniqueId, out var translator, out error))
        {
            return false;
        }

        var trans = translator.Get(key, tokens);
        if (trans.HasValue())
        {
            value = trans;
            return true;
        }
        
        error = $"Failed to get translation for key '{key}' from mod '{uniqueId}'";
        value = null;
        return false;
    }

    public static bool TryGetModTranslation(string id, string key, [NotNullWhen(true)] out Translation? value, out string? error)
    {
        value = null;
        error = null;
        return TryGetModTranslation(id, key, null, out value, out error);
    }
}