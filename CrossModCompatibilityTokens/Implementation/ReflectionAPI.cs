using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CrossModCompatibilityTokens.API;
using CrossModCompatibilityTokens.Helpers;
using CrossModCompatibilityTokens.Readers;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Framework;
using StardewValley.Extensions;

namespace CrossModCompatibilityTokens.Implementation;

public partial class CrossModCompatibilityToolsAPI : ICrossModCompatibilityToolsAPI
{
    public bool TryGetModEntry(IModInfo mod, [NotNullWhen(true)] out IMod? modInstance, out string? error)
    {
        modInstance = null;
        error = null;
        if (!ModList.TryGetMod(mod, out modInstance, out error))
        {
            return false;
        }
        return true;
    }

    public bool TryGetContentPack(IModInfo mod, [NotNullWhen(true)] out IContentPack? pack, out string? error)
    {
        pack = null;
        error = null;
        if (!ModList.TryGetContentPack(mod, out pack, out error))
        {
            return false;
        }
        return true;
    }

    public bool TryGetModHelper(IModInfo mod, [NotNullWhen(true)] out IModHelper? helper, out string? error)
    {
        helper = null;
        error = null;
        if (!ModList.TryGetModHelper(mod, out helper, out error))
        {
            return false;
        }
        return true;
    }

    public bool TryGetModAssembly(IModInfo mod, [NotNullWhen(true)] out Assembly? assembly, out string? error)
    {
        assembly = null;
        error = null;
        if (!ModList.TryGetModAssembly(mod, out assembly, out error))
        {
            return false;
        }
        return true;
    }

    public bool TryGetTypeFromMod(IModInfo mod, string typeName, [NotNullWhen(true)] out Type? type, out string? error)
    {
        type = null;
        error = null;
        if (!ModList.TryGetModAssembly(mod, out var assembly, out error))
        {
            return false;
        }
        
        type = assembly.GetType(typeName);
        type ??= assembly.GetTypes().FirstOrDefault(t => t.FullName.EqualsIgnoreCase(typeName));
        type ??= assembly.GetTypes().FirstOrDefault(t => t.Name.EqualsIgnoreCase(typeName));
        
        if (type is null)
        {
            error = $"Type '{typeName}' not found in mod with UniqueID '{mod.Manifest.UniqueID}'";
            return false;
        }
        return true;
    }

    public bool TryGetMethodFromMod(IModInfo mod, string? typeName, string methodName, [NotNullWhen(true)] out MethodInfo? method, out string? error)
    {
        method = null;
        error = null;
        if (!ModList.TryGetModAssembly(mod, out var assembly, out error))
        {
            return false;
        }

        Type? type = null;
        if (typeName is not null && !TryGetTypeFromMod(mod, typeName, out type, out error))
        {
            return false;
        }

        if (type is not null)
        {
            method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (method is not null)
            {
                return true;
            }
        }
        
        method = assembly.GetTypes().FirstOrDefault((t => t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) is not null))?.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        if (method is not null)
        {
            return true;
        }
        
        error = typeName is not null ? $"Method '{methodName}' not found in type '{typeName}'" : $"Method '{methodName}' not found in mod with UniqueID '{mod.Manifest.UniqueID}'";
        return false;
    }

    public IMod? GetModEntry(IModInfo mod)
    {
        if (!TryGetModEntry(mod, out var modEntry, out var error))
        {
            Log.Error(error);
            return null;
        }
        return modEntry;
    }

    public IContentPack? GetContentPack(IModInfo mod)
    {
        if (!TryGetContentPack(mod, out var contentPack, out var error))
        {
            Log.Error(error);
            return null;
        }
        return contentPack;
    }

    public IModHelper? GetModHelper(IModInfo mod)
    {
        if (!TryGetModHelper(mod, out var modHelper, out var error))
        {
            Log.Error(error);
            return null;
        }
        return modHelper;
    }

    public Assembly? GetModAssembly(IModInfo mod)
    {
        if (!TryGetModAssembly(mod, out var modAssembly, out var error))
        {
            Log.Error(error);
            return null;
        }
        return modAssembly;
    }

    public Type? GetTypeFromMod(IModInfo mod, string typeName)
    {
        if (!TryGetTypeFromMod(mod, typeName, out var type, out var error))
        {
            Log.Error(error);
            return null;
        }
        return type;
    }

    public MethodInfo? GetMethodFromMod(IModInfo mod, string? typeName, string methodName)
    {
        if (!TryGetMethodFromMod(mod, typeName, methodName, out var method, out var error))
        {
            Log.Error(error);
            return null;
        }
        return method;
    }
}