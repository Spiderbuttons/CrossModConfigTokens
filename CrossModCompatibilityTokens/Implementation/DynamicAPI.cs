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
    public bool TryGetDynamicTokenValues(IModInfo mod, string token, [NotNullWhen(true)] out IEnumerable<string>? values, out string? error)
    {
        values = null;
        error = null;
        if (!DynamicReader.DynamicCache.TryGetValue(mod.Manifest.UniqueID, out var manager) || !manager.TryGetValuesNoCache(token, out values, out error))
        {
            return false;
        }
        return true;
    }

    public IEnumerable<string>? GetDynamicTokenValues(IModInfo mod, string token)
    {
        if (!TryGetDynamicTokenValues(mod, token, out var values, out var error))
        {
            Log.Error(error);
            return null;
        }
        return values;
    }
}