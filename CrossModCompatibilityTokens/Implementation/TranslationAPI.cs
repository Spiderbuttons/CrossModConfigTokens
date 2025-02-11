using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CrossModCompatibilityTokens.API;
using CrossModCompatibilityTokens.Helpers;
using CrossModCompatibilityTokens.Readers;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Framework;

namespace CrossModCompatibilityTokens.Implementation;

public partial class CrossModCompatibilityToolsAPI : ICrossModCompatibilityToolsAPI
{
    public bool TryGetTranslation(IModInfo mod, string transKey, [NotNullWhen(true)] out Translation? transValue, out string? error)
    {
        transValue = null;
        error = null;
        if (!TranslationReader.TryGetModTranslation(mod.Manifest.UniqueID, transKey, out transValue, out error))
        {
            return false;
        }
        return true;
    }

    public bool TryGetTranslation(IModInfo mod, string transKey, object? tokens, [NotNullWhen(true)] out Translation? transValue, out string? error)
    {
        transValue = null;
        error = null;
        if (!TranslationReader.TryGetModTranslation(mod.Manifest.UniqueID, transKey, tokens, out transValue, out error))
        {
            return false;
        }
        return true;
    }

    public bool TryGetTranslationHelper(IModInfo mod, [NotNullWhen(true)] out ITranslationHelper? translator, out string? error)
    {
        translator = null;
        error = null;
        if (!TranslationReader.TryGetModTranslationHelper(mod, out translator, out error))
        {
            return false;
        }
        return true;
    }

    public Translation? GetTranslation(IModInfo mod, string transKey)
    {
        if (!TryGetTranslation(mod, transKey, out var transValue, out var error))
        {
            return null;
        }
        return transValue;
    }

    public Translation? GetTranslation(IModInfo mod, string transKey, object? tokens)
    {
        if (!TryGetTranslation(mod, transKey, tokens, out var transValue, out var error))
        {
            return null;
        }
        return transValue;
    }

    public ITranslationHelper? GetTranslationHelper(IModInfo mod)
    {
        if (!TryGetTranslationHelper(mod, out var translator, out var error))
        {
            return null;
        }
        return translator;
    }
}