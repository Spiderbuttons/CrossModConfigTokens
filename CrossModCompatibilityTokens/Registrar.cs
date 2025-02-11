using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CrossModCompatibilityTokens.API;
using CrossModCompatibilityTokens.Helpers;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace CrossModCompatibilityTokens;

public static class Registrar
{ 
    public static Dictionary<string, IDictionary<string, ICrossModAction>> ModActions { get; } = new();
    
    public static bool TryGetAction(IModInfo mod, string actionId, [NotNullWhen(true)] out ICrossModAction? action, out string? error)
    {
        error = null;
        action = null;
        if (!TryGetActions(mod, out var actions, out error))
        {
            return false;
        }

        if (!actions.TryGetValue(actionId, out action))
        {
            error = $"Action with ID '{actionId}' not found for mod with UniqueID '{mod.Manifest.UniqueID}'";
            return false;
        }
        
        return true;
    }
    
    public static bool TryGetActions(IModInfo mod, [NotNullWhen(true)] out IDictionary<string, ICrossModAction>? actions, out string? error)
    {
        error = null;
        actions = null;
        if (!TryGetActionsFromRegistrar(mod, out actions, out error))
        {
            if (!TryGetActionsFromEntry(mod, out actions, out error))
            {
                return false;
            }
            ModActions[mod.Manifest.UniqueID] = actions;
        }

        return true;
    }

    public static bool TryGetActionsFromRegistrar(IModInfo mod, [NotNullWhen(true)] out IDictionary<string, ICrossModAction>? actions, out string? error)
    {
        error = null;
        actions = null;
        if (!ModActions.TryGetValue(mod.Manifest.UniqueID, out actions))
        {
            error = $"Mod with UniqueID '{mod.Manifest.UniqueID}' has no actions registered";
            return false;
        }
        
        return true;
    }

    public static bool TryGetActionsFromEntry(IModInfo mod, [NotNullWhen(true)] out IDictionary<string, ICrossModAction>? actions, out string? error)
    {
        error = null;
        actions = null;
        if (!ModList.TryGetMod(mod, out var modInstance, out error))
        {
            return false;
        }
        
        var actionsList = modInstance.GetType().GetField("CrossModCompatibilityTools")?.GetValue(modInstance) ?? modInstance.GetType().GetProperty("CrossModCompatibilityTools")?.GetValue(modInstance);
        if (actionsList is not IList<Action> list)
        {
            error = $"Mod with UniqueID '{mod.Manifest.UniqueID}' has no actions registered";
            return false;
        }
        
        actions = new Dictionary<string, ICrossModAction>();
        foreach (var item in list)
        {
            var action = new CrossModAction(mod, item.GetMethodInfo().Name, item, null);
            actions[item.GetMethodInfo().Name] = action;
        }
        
        return true;
    }
}