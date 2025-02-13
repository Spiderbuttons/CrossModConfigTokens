using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrossModCompatibilityTokens.API;
using CrossModCompatibilityTokens.Helpers;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.Implementation;

public partial class CrossModCompatibilityToolsAPI : ICrossModCompatibilityToolsAPI
{ 
    public bool TryRegisterAction(IManifest manifest, string? consumer, string actionId, Action action, Dictionary<string, object>? customFields, out string? error)
    {
        var modInfo = ModEntry.ModHelper.ModRegistry.Get(manifest.UniqueID)!;
        return Registrar.TryRegisterAction(manifest, new CrossModAction(modInfo, consumer, actionId, action, customFields), out error);
    }

    public bool TryGetActionFromMod(IModInfo mod, string actionId, [NotNullWhen(true)] out ICrossModAction? action, out string? error, bool reflectIfNecessary = false)
    {
        error = null;
        action = null;
        return Registrar.TryGetAction(mod, actionId, out action, out error, reflectIfNecessary);
    }
    
    public bool TryGetActionsFromMod(IModInfo mod, [NotNullWhen(true)] out IDictionary<string, ICrossModAction>? actions, out string? error)
    {
        error = null;
        actions = null;
        return Registrar.TryGetActions(mod, out actions, out error);
    }

    public bool TryGetActionsForConsumer(IManifest consumer, [NotNullWhen(true)] out List<ICrossModAction>? actions, out string? error)
    {
        error = null;
        actions = null;
        List<ICrossModAction> actionsList = new();
        foreach (var list in Registrar.ModActions.Values)
        {
            actionsList.AddRange(list.Values.Where(action => action.IntendedConsumer?.Equals(consumer.UniqueID) ?? false));
        }
        if (actionsList.Count > 0)
        {
            actions = actionsList;
            return true;
        }
        error = $"No actions found for consumer with UniqueID '{consumer.UniqueID}'";
        return false;
    }

    public bool TryInvokeActionFromMod(IModInfo mod, string actionId, out string? error, bool reflectIfNecessary = false)
    {
        error = null;
        if (!Registrar.TryGetAction(mod, actionId, out var action, out error, reflectIfNecessary))
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

    public void RegisterAction(IManifest mod, string? consumer, string actionId, Action action, Dictionary<string, object>? customFields)
    {
        if (!TryRegisterAction(mod, consumer, actionId, action, customFields, out var error))
        {
            Log.Error(error);
        }
    }

    public ICrossModAction? GetActionFromMod(IModInfo mod, string actionId, bool reflectIfNecessary = false)
    {
        if (!TryGetActionFromMod(mod, actionId, out var action, out var error, reflectIfNecessary))
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

    public List<ICrossModAction>? GetActionsForConsumer(IManifest consumer)
    {
        if (!TryGetActionsForConsumer(consumer, out var actions, out var error))
        {
            Log.Error(error);
            return null;
        }
        return actions;
    }

    public void InvokeActionFromMod(IModInfo mod, string actionId, bool reflectIfNecessary = false)
    {
        if (!TryInvokeActionFromMod(mod, actionId, out var error, reflectIfNecessary))
        {
            Log.Error(error);
        }
    }
}

public class CrossModAction(IModInfo mod, string? consumer, string id, Action action, Dictionary<string, object>? customFields = null) : ICrossModAction
{
    public IModInfo Mod { get; } = mod;
    public string? IntendedConsumer { get; } = consumer;
    public string Id { get; } = id;
    public Action Action { get; } = action;
    public Dictionary<string, object>? CustomFields { get; } = customFields;
}