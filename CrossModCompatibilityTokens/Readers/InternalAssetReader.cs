using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.Readers;

public static class InternalAssetReader
{
    public class InternalAssetManager(string uniqueId)
    {
        private string ModId { get; } = uniqueId;
        private Dictionary<string, IAssetName> Cache { get; } = new();

        public bool TryGetValue(string path, [NotNullWhen(true)] out IAssetName? value, out string? error)
        {
            error = null;
            value = null;
            return Cache.TryGetValue(path, out value) || TryGetValueNoCache(path, out value, out error);
        }

        public bool TryGetValueNoCache(string path, [NotNullWhen(true)] out IAssetName? value, out string? error)
        {
            error = null;
            value = null;
            if (!TryGetInternalAssetName(ModId, path, out value, out error))
                return false;
            
            Cache[path] = value;
            return true;
        }
        
        public IEnumerable<string> GetCachedPaths()
        {
            return Cache.Keys;
        }
    }
    
    private static bool TryGetModContent(string uniqueId, [NotNullWhen(true)] out IModContentHelper? content, out string? error)
    {
        content = null;
        error = null;
        if (ModList.TryGetMod(uniqueId, out var mod, out error))
        {
            content = mod.Helper.ModContent;
            return true;
        }

        if (ModList.TryGetContentPack(uniqueId, out var pack, out error))
        {
            content = pack.ModContent;
            return true;
        }

        return false;
    }
    
    public static bool TryGetModContent(IModInfo mod, [NotNullWhen(true)] out IModContentHelper? content, out string? error)
    {
        return TryGetModContent(mod.Manifest.UniqueID, out content, out error);
    }
    
    private static bool TryGetInternalAssetName(string uniqueId, string path, [NotNullWhen(true)] out IAssetName? asset, out string? error)
    {
        asset = null;
        error = null;
        if (!TryGetModContent(uniqueId, out var content, out error))
        {
            return false;
        }

        if (!content.DoesAssetExist<object>(path))
        {
            error = $"Failed to get internal asset name for path '{path}' from mod '{uniqueId}'";
            return false;
        }

        asset = content.GetInternalAssetName(path);
        return true;
    }
}