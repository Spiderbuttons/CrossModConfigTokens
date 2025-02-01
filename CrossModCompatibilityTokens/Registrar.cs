using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.AccessControl;
using CrossModCompatibilityTokens.Helpers;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.ModHelpers;

namespace CrossModCompatibilityTokens;

public static class Registrar
{
    public static ModRegistry Registry { get; set; }

    static Registrar()
    {
        Registry = SCore.Instance.ModRegistry;
    }

    public static bool AreAllModsLoaded()
    {
        return Registry.AreAllModsLoaded;
    }
    
    public static bool TryGetModMetadata(string uniqueId, [NotNullWhen(true)] out IModMetadata? mod, out string? error)
    {
        mod = null;
        error = null;
        if (!AreAllModsLoaded())
        {
            error = "SMAPI has not finished loading mods yet!";
            return false;
        }
        if (Registry.Get(uniqueId) is null)
        {
            error = $"{uniqueId} does not exist in SMAPI's mod registry!";
            return false;
        }
        mod = Registry.Get(uniqueId)!;
        return true;
    }

    public static bool TryGetMod(string uniqueId, [NotNullWhen(true)] out IMod? mod, out string? error)
    {
        mod = null;
        error = null;
        if (!TryGetModMetadata(uniqueId, out var metadata, out error))
        {
            return false;
        }

        if (!metadata.IsContentPack)
        {
            mod = metadata.Mod!;
            return true;
        }
        
        error = $"{uniqueId} is a content pack.";
        return false;
    }

    public static bool TryGetContentPack(string uniqueId, [NotNullWhen(true)] out IContentPack? pack, out string? error)
    {
        pack = null;
        error = null;
        if (!TryGetModMetadata(uniqueId, out var metadata, out error))
        {
            return false;
        }
        
        if (metadata.IsContentPack)
        {
            pack = metadata.ContentPack!;
            return true;
        }
        
        error = $"{uniqueId} is not a content pack!";
        return false;
    }

    public static bool TryGetModHelper(string uniqueId, [NotNullWhen(true)] out IModHelper? helper, out string? error)
    {
        helper = null;
        error = null;
        if (!TryGetMod(uniqueId, out var mod, out error))
        {
            return false;
        }

        helper = mod.Helper;
        return true;
    }

    public static bool TryGetModAssembly(string uniqueId, [NotNullWhen(true)] out Assembly? assembly, out string? error)
    {
        assembly = null;
        error = null;
        if (!TryGetMod(uniqueId, out var mod, out error))
        {
            return false;
        }

        assembly = mod.GetType().Assembly;
        return true;
    }
}