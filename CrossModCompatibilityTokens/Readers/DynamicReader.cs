using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using CrossModCompatibilityTokens.Helpers;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace CrossModCompatibilityTokens.Readers;

public static class DynamicReader
{
    public class DynamicTokenManager(string uniqueId)
    {
        private string ModId { get; } = uniqueId;
        private Dictionary<string, IEnumerable<string>> Cache { get; } = new();

        public bool TryGetValues(string name, [NotNullWhen(true)] out IEnumerable<string>? values, out string? error)
        {
            error = null;
            values = null;
            return Cache.TryGetValue(name, out values) || TryGetValuesNoCache(name, out values, out error);
        }

        public bool TryGetValuesNoCache(string name, [NotNullWhen(true)] out IEnumerable<string>? values, out string? error)
        {
            error = null;
            values = null;
            if (!TryGetDynamicTokenValues(ModId, name, out values, out error))
                return false;
            
            Cache[name] = values;
            return true;
        }
        
        public IEnumerable<string> GetCachedNames()
        {
            return Cache.Keys;
        }
    }
    
    public static bool TryGetTokenManager([NotNullWhen(true)] out TokenManager? manager, out string? error)
    {
        manager = null;
        error = null;
        if (!Registrar.TryGetMod("Pathoschild.ContentPatcher", out var mod, out error) || mod is not ContentPatcher.ModEntry cp)
        {
            return false;
        }

        manager = cp.ScreenManager.Value.TokenManager;
        return true;
    }

    public static bool TryGetTokenContext(string uniqueId, [NotNullWhen(true)] out ModTokenContext? context, out string? error)
    {
        context = null;
        error = null;
        if (!TryGetTokenManager(out var manager, out error) || !Registrar.TryGetContentPack(uniqueId, out var pack, out error))
        {
            return false;
        }

        context = manager.TrackLocalTokens(pack);
        return true;
    }

    public static bool TryGetDynamicToken(string uniqueId, string name, [NotNullWhen(true)] out IToken? token, out string? error)
    {
        token = null;
        error = null;
        if (!TryGetTokenContext(uniqueId, out var context, out error))
        {
            return false;
        }

        token = context.GetToken(name, enforceContext: false);
        return token is not null;
    }

    public static bool TryGetDynamicTokenValues(string id, string key, [NotNullWhen(true)] out IEnumerable<string>? values, out string? error)
    {
        values = null;
        error = null;
        if (!TryGetDynamicToken(id, key, out var token, out error))
        {
            return false;
        }

        values = token.GetValues(new EmptyInputArguments());
        return true;
    }
}