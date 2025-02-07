using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CrossModCompatibilityTokens.Helpers;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

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
    
    private static bool TryGetTokenManager([NotNullWhen(true)] out object? manager, out string? error)
    {
        manager = null;
        error = null;
        if (!Registrar.TryGetMod("Pathoschild.ContentPatcher", out var mod, out error))
        {
            return false;
        }
        
        // manager = mod.ScreenManager.Value.TokenManager;
        var ScreenManager = ModEntry.ModHelper.Reflection.GetField<object>(mod, "ScreenManager").GetValue();
        var Screen = ScreenManager.GetType().GetProperty("Value")!.GetValue(ScreenManager); // Can't use the Helper here because SMAPI won't reflect into its own internals for us ):
        var TokenManager = ModEntry.ModHelper.Reflection.GetProperty<object>(Screen!, "TokenManager").GetValue();
        manager = TokenManager;
        return true;
    }

    private static bool TryGetTokenContext(string uniqueId, [NotNullWhen(true)] out object? context, out string? error)
    {
        context = null;
        error = null;
        if (!TryGetTokenManager(out var manager, out error) || !Registrar.TryGetContentPack(uniqueId, out var pack, out error))
        {
            return false;
        }

        // context = manager.TrackLocalTokens(pack);
        context = ModEntry.ModHelper.Reflection.GetMethod(manager, "TrackLocalTokens").Invoke<object>(pack);
        return true;
    }

    private static bool TryGetDynamicToken(string uniqueId, string name, [NotNullWhen(true)] out object? token, out string? error)
    {
        token = null;
        error = null;
        if (!TryGetTokenContext(uniqueId, out var context, out error))
        {
            return false;
        }

        // token = context.GetToken(name, enforceContext: false);
        token = ModEntry.ModHelper.Reflection.GetMethod(context, "GetToken").Invoke<object>(name, false);
        return true;
    }

    public static bool TryGetDynamicTokenValues(string uniqueId, string key, [NotNullWhen(true)] out IEnumerable<string>? values, out string? error)
    {
        values = null;
        error = null;
        if (!TryGetDynamicToken(uniqueId, key, out var token, out error))
        {
            return false;
        }

        // values = token.GetValues(new EmptyInputArguments());
        var EmptyInputArguments = Activator.CreateInstance(Type.GetType("ContentPatcher.Framework.Tokens.EmptyInputArguments, ContentPatcher")!, []);
        values = ModEntry.ModHelper.Reflection.GetMethod(token, "GetValues").Invoke<IEnumerable<string>>(EmptyInputArguments);
        return true;
    }
}