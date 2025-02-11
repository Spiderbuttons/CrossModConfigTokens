using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.Readers;

public static class TranslationReader
{
    private static bool TryGetModTranslator(string uniqueId, [NotNullWhen(true)] out ITranslationHelper? translator, out string? error)
    {
        translator = null;
        error = null;
        if (!ModList.TryGetModMetadata(uniqueId, out var metadata, out error))
        {
            return false;
        }

        translator = metadata.Translations!;
        return true;
    }
    
    public static bool TryGetModTranslator(IModInfo mod, [NotNullWhen(true)] out ITranslationHelper? translator, out string? error)
    {
        return TryGetModTranslator(mod.Manifest.UniqueID, out translator, out error);
    }
    
    private static bool TryGetModTranslation(string uniqueId, string key, object? tokens, out string? value, out string? error)
    {
        value = null;
        error = null;
        if (!TryGetModTranslator(uniqueId, out var translator, out error))
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

    public static bool TryGetModTranslation(string id, string key, out string? value, out string? error)
    {
        value = null;
        error = null;
        return TryGetModTranslation(id, key, null, out value, out error);
    }
}