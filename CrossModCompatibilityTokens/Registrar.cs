using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CrossModCompatibilityTokens.API;
using CrossModCompatibilityTokens.Helpers;
using CrossModCompatibilityTokens.Implementation;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;

namespace CrossModCompatibilityTokens;

public static class Registrar
{ 
    public static Dictionary<string, IDictionary<string, ICrossModAction>> ModActions { get; } = new();
    
    public static bool TryRegisterAction(IManifest manifest, ICrossModAction action, out string? error)
    {
        error = null;
        if (!ModActions.TryGetValue(manifest.UniqueID, out var actions))
        {
            actions = new Dictionary<string, ICrossModAction>();
            ModActions[manifest.UniqueID] = actions;
        }

        if (!actions.TryAdd(action.Id, action))
        {
            error = $"Action '{action.Id}' is already registered with Cross-Mod Compatibility Tools.";
            return false;
        }

        return true;

    }
    
    public static bool TryGetAction(IModInfo mod, string actionId, [NotNullWhen(true)] out ICrossModAction? action, out string? error, bool reflectIfNecessary)
    {
        error = null;
        action = null;
        if (!TryGetActions(mod, out var actions, out error))
        {
            return false;
        }

        if (!actions.TryGetValue(actionId, out action) && !actions.Values.Any(ac => QualifyMethodName(ac.Action.Method).EqualsIgnoreCase(actionId)) && !actions.Values.Any(ac => ac.Action.Method.Name.EqualsIgnoreCase(actionId)))
        {
            if (reflectIfNecessary && !TryGetActionFromReflection(mod, actionId, out action, out error))
            {
                return false;
            }
            
            if (action is null)
            {
                error = $"Action with ID '{actionId}' not found for mod with UniqueID '{mod.Manifest.UniqueID}'";
                return false;
            }
        }
        
        action ??= actions.Values.FirstOrDefault(ac => QualifyMethodName(ac.Action.Method).EqualsIgnoreCase(actionId)) ?? actions.Values.First(ac => ac.Action.Method.Name.EqualsIgnoreCase(actionId));
        return true;
    }

    public static bool TryGetActionFromReflection(IModInfo mod, string qualifiedName,
        [NotNullWhen(true)] out ICrossModAction? action, out string? error)
    {
        action = null;
        error = null;
        if (!ModList.TryGetModAssembly(mod, out var assembly, out error))
        {
            return false;
        }
        
        var typeName = qualifiedName.Split(':')[0];
        var methodName = qualifiedName.Split(':')[1];
        var type = assembly.GetType(typeName);
        if (type == null)
        {
            error = $"Type '{typeName}' not found in assembly '{assembly.GetName().Name}'";
            return false;
        }
        
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        if (method == null)
        {
            error = $"Method '{methodName}' not found in type '{typeName}'";
            return false;
        }

        if (!method.IsStatic)
        {
            error = $"Method '{methodName}' is not static, unable to automatically register it as an action";
            return false;
        }
        
        action = new CrossModAction(mod, null, QualifyMethodName(method), method.CreateDelegate<Action>());
        ModActions[mod.Manifest.UniqueID][action.Id] = action;
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

    private static bool TryGetActionsFromRegistrar(IModInfo mod, [NotNullWhen(true)] out IDictionary<string, ICrossModAction>? actions, out string? error)
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

    private static bool TryGetActionsFromEntry(IModInfo mod, [NotNullWhen(true)] out IDictionary<string, ICrossModAction>? actions, out string? error)
    {
        error = null;
        actions = null;
        if (!ModList.TryGetMod(mod, out var modInstance, out error))
        {
            return false;
        }
        
        var actionsList = modInstance.GetType().GetField("CrossModCompatibilityTools")?.GetValue(modInstance) ?? modInstance.GetType().GetProperty("CrossModCompatibilityTools")?.GetValue(modInstance);
        if (actionsList is not IDictionary<string, Action> list)
        {
            error = $"Mod with UniqueID '{mod.Manifest.UniqueID}' has no actions registered";
            return false;
        }
        
        actions = new Dictionary<string, ICrossModAction>();
        foreach (var item in list)
        {
            var action = new CrossModAction(mod, null, item.Key, item.Value);
            actions[item.Key] = action;
        }
        
        return true;
    }

    private static string QualifyMethodName(MethodInfo method)
    {
        return method?.DeclaringType?.FullName + ":" + method?.Name;
    }
}