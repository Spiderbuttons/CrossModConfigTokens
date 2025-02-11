using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CrossModCompatibilityTokens.API;
using CrossModCompatibilityTokens.Helpers;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.Implementation;

public partial class CrossModCompatibilityToolsAPI : ICrossModCompatibilityToolsAPI
{ 
    public bool TryRegisterAction(IManifest manifest, string id, Action action, Dictionary<string, object>? customFields, out string? error)
    {
        var modInfo = ModEntry.ModHelper.ModRegistry.Get(manifest.UniqueID)!;
        return TryRegisterAction(manifest, new CrossModAction(modInfo, id, action, customFields), out error);
    }

    public bool TryRegisterAction(IManifest manifest, ICrossModAction action, out string? error)
    {
        error = null;
        if (!Registrar.ModActions.TryGetValue(manifest.UniqueID, out var actions))
        {
            actions = new Dictionary<string, ICrossModAction>();
            Registrar.ModActions[manifest.UniqueID] = actions;
        }

        if (!actions.TryAdd(action.Id, action))
        {
            error = $"Action '{action.Id}' is already registered with Cross-Mod Compatibility Tools.";
            return false;
        }

        return true;

    }

    public bool TryGetActionFromMod(IModInfo mod, string actionId, [NotNullWhen(true)] out ICrossModAction? action, out string? error)
    {
        error = null;
        action = null;
        return Registrar.TryGetAction(mod, actionId, out action, out error);
    }
    
    public bool TryGetActionsFromMod(IModInfo mod, [NotNullWhen(true)] out IDictionary<string, ICrossModAction>? actions, out string? error)
    {
        error = null;
        actions = null;
        return Registrar.TryGetActions(mod, out actions, out error);
    }
    
    public bool TryInvokeActionFromMod(IModInfo mod, string actionId, out string? error)
    {
        error = null;
        if (!Registrar.TryGetAction(mod, actionId, out var action, out error))
        {
            return false;
        }
        
        try
        {
            action.Action.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            error = $"Error invoking action with ID '{action.Id}' for mod with UniqueID '{mod.Manifest.UniqueID}': {ex}";
            return false;
        }
    }

    public void RegisterAction(IManifest mod, string id, Action action, Dictionary<string, object>? customFields)
    {
        if (!TryRegisterAction(mod, id, action, customFields, out var error))
        {
            Log.Error(error);
        }
    }

    public ICrossModAction? GetActionFromMod(IModInfo mod, string actionId)
    {
        if (!TryGetActionFromMod(mod, actionId, out var action, out var error))
        {
            Log.Error(error);
            return null;
        }
        return action;
    }

    public IDictionary<string, ICrossModAction>? GetActionsFromMod(IModInfo mod)
    {
        if (!TryGetActionsFromMod(mod, out var actions, out var error))
        {
            Log.Error(error);
            return null;
        }
        return actions;
    }

    public void InvokeActionFromMod(IModInfo mod, string actionId)
    {
        if (!TryInvokeActionFromMod(mod, actionId, out var error))
        {
            Log.Error(error);
        }
    }
}

public class CrossModAction(IModInfo mod, string id, Action action, Dictionary<string, object>? customFields = null) : ICrossModAction
{
    public IModInfo Mod { get; } = mod;
    public string Id { get; } = id;
    public Action Action { get; } = action;
    public Dictionary<string, object>? CustomFields { get; } = customFields;
}