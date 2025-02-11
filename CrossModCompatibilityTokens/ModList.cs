using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Framework;

namespace CrossModCompatibilityTokens;

public static class ModList
{
    private static ModRegistry ModRegistry { get; }

    static ModList()
    {
        ModRegistry = SCore.Instance.ModRegistry;
    }

    public static bool AreAllModsLoaded()
    {
        return ModRegistry.AreAllModsLoaded;
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
        if (ModRegistry.Get(uniqueId) is null)
        {
            error = $"{uniqueId} does not exist in SMAPI's mod registry!";
            return false;
        }
        mod = ModRegistry.Get(uniqueId)!;
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
    
    public static bool TryGetMod(IModInfo modInfo, [NotNullWhen(true)] out IMod? mod, out string? error)
    {
        return TryGetMod(modInfo.Manifest.UniqueID, out mod, out error);
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
    
    public static bool TryGetContentPack(IModInfo modInfo, [NotNullWhen(true)] out IContentPack? pack, out string? error)
    {
        return TryGetContentPack(modInfo.Manifest.UniqueID, out pack, out error);
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
    
    public static bool TryGetModHelper(IModInfo modInfo, [NotNullWhen(true)] out IModHelper? helper, out string? error)
    {
        return TryGetModHelper(modInfo.Manifest.UniqueID, out helper, out error);
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
    
    public static bool TryGetModAssembly(IModInfo modInfo, [NotNullWhen(true)] out Assembly? assembly, out string? error)
    {
        return TryGetModAssembly(modInfo.Manifest.UniqueID, out assembly, out error);
    }
}