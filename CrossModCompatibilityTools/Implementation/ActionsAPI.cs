using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrossModCompatibilityTools.API;
using CrossModCompatibilityTools.Helpers;
using Nanoray.Pintail;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace CrossModCompatibilityTools.Implementation;

public partial class CrossModCompatibilityToolsAPI : ICrossModCompatibilityToolsAPI
{ 
    public bool TryRegisterAction(IManifest manifest, string actionId, string? category, Func<string>? name, Func<string>? description, Action action, Dictionary<string, string>? customFields, object? customData, [NotNullWhen(false)] out string? error)
    {
        var modInfo = ModEntry.ModHelper.ModRegistry.Get(manifest.UniqueID)!;
        return Registrar.TryRegisterAction(manifest, new CrossModAction(Registrar.ProxyManager, modInfo, actionId, category, name, description, action, customFields, customData), out error);
    }

    public bool TryRegisterAction(IManifest manifest, string actionId, Func<string> name, Func<string> description, Action action, [NotNullWhen(false)] out string? error)
    {
        throw new NotImplementedException();
    }

    public bool TryRegisterAction(IManifest manifest, Action action, [NotNullWhen(false)] out string? error)
    {
        return Registrar.TryRegisterAction(manifest, new CrossModAction(Registrar.ProxyManager, ModEntry.ModHelper.ModRegistry.Get(manifest.UniqueID)!, action.Method.Name, null, null, null, action, null, null), out error);
    }

    public bool TryGetActionFromMod(IModInfo mod, string actionId, [NotNullWhen(true)] out ICrossModAction? action, out string? error, bool reflectIfNecessary = false)
    {
        error = null;
        action = null;
        return Registrar.TryGetAction(mod, actionId, out action, out error, reflectIfNecessary);
    }
    
    public bool TryGetActionsFromModAsDict(IModInfo mod, [NotNullWhen(true)] out IReadOnlyDictionary<string, ICrossModAction>? actions, [NotNullWhen(false)] out string? error)
    {
        error = null;
        actions = null;
        if (!Registrar.TryGetActions(mod, out var actionsDict, out error))
        {
            return false;
        }
        
        actions = (IReadOnlyDictionary<string, ICrossModAction>)actionsDict;
        return true;
    }

    public bool TryGetActionsFromModAsList(IModInfo mod,
        [NotNullWhen(true)] out IReadOnlyList<ICrossModAction>? actions, [NotNullWhen(false)] out string? error)
    {
        error = null;
        actions = null;
        if (!Registrar.TryGetActions(mod, out var actionsDict, out error))
        {
            return false;
        }

        actions = actionsDict.Values.ToImmutableList();
        return true;
    }

    public bool TryGetActionsByCategory(string category,
        [NotNullWhen(true)] out IReadOnlyList<ICrossModAction>? actions, [NotNullWhen(false)] out string? error)
    {
        error = null;
        actions = null;
        if (!Registrar.TryGetAllActions(out var allActions, out error))
        {
            return false;
        }

        actions = allActions.Where(x => x.Category.EqualsIgnoreCase(category)).ToList();
        return true;
    }

    public bool TryGetActionsAsDict([NotNullWhen(true)] out IReadOnlyDictionary<string, ICrossModAction>? actions,
        [NotNullWhen(false)] out string? error)
    {
        error = null;
        actions = null;
        if (!Registrar.TryGetAllActions(out var allActions, out error))
        {
            return false;
        }

        actions = allActions.ToDictionary(x => x.Id, x => x);
        return true;
    }
    
    public bool TryGetActionsAsList([NotNullWhen(true)] out IReadOnlyList<ICrossModAction>? actions,
        [NotNullWhen(false)] out string? error)
    {
        error = null;
        actions = null;
        if (!Registrar.TryGetAllActions(out var allActions, out error))
        {
            return false;
        }

        actions = allActions;
        return true;
    }

    public bool TryInvokeActionFromMod(IModInfo mod, string actionId, [NotNullWhen(false)] out string? error, bool reflectIfNecessary = false)
    {
        error = null;
        if (!Registrar.TryGetAction(mod, actionId, out var action, out error, reflectIfNecessary))
        {
            return false;
        }
        
        try
        {
            action.PerformAction();
            return true;
        }
        catch (Exception ex)
        {
            error = $"Error invoking action with ID '{action.Id}' for mod with UniqueID '{mod.Manifest.UniqueID}': {ex}";
            return false;
        }
    }

    public bool TryInvokeActionById(string actionId, [NotNullWhen(false)] out string? error)
    {
        error = null;
        if (!Registrar.TryGetAllActions(out var allActions, out error))
        {
            return false;
        }
        
        var action = allActions.FirstOrDefault(x => x.Id.EqualsIgnoreCase(actionId));
        if (action is null)
        {
            error = $"Action with ID '{actionId}' not found.";
            return false;
        }
        
        try
        {
            action.PerformAction();
            return true;
        }
        catch (Exception ex)
        {
            error = $"Error invoking action with ID '{action.Id}': {ex}";
            return false;
        }
    }

    public void RegisterAction(IManifest manifest, string actionId, string? category, Func<string>? name, Func<string>? description, Action action, Dictionary<string, string>? customFields, object? customData)
    {
        if (!TryRegisterAction(manifest, actionId, category, name, description, action, customFields, customData, out var error))
        {
            Log.Error(error);
        }
    }

    public void RegisterAction(IManifest manifest, string actionId, Func<string>? name, Func<string>? description, Action action)
    {
        throw new NotImplementedException();
    }

    public void RegisterAction(IManifest manifest, Action action)
    {
        if (!TryRegisterAction(manifest, action, out var error))
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

    public IReadOnlyDictionary<string, ICrossModAction>? GetActionsFromModAsDict(IModInfo mod)
    {
        if (!TryGetActionsFromModAsDict(mod, out var actions, out var error))
        {
            Log.Error(error);
            return null;
        }
        return actions;
    }

    public IReadOnlyList<ICrossModAction>? GetActionsFromModAsList(IModInfo mod)
    {
        if (!TryGetActionsFromModAsList(mod, out var actions, out var error))
        {
            Log.Error(error);
            return null;
        }
        return actions;
    }

    public IReadOnlyList<ICrossModAction>? GetActionsByCategory(string category)
    {
        if (!TryGetActionsByCategory(category, out var actions, out var error))
        {
            Log.Error(error);
            return null;
        }
        return actions;
    }

    public IReadOnlyDictionary<string, ICrossModAction>? GetActionsAsDict()
    {
        if (!TryGetActionsAsDict(out var actions, out var error))
        {
            Log.Error(error);
            return null;
        }
        return actions;
    }

    public IReadOnlyList<ICrossModAction>? GetActionsAsList()
    {
        if (!TryGetActionsAsList(out var actions, out var error))
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

    public void InvokeActionById(string actionId)
    {
        if (!TryInvokeActionById(actionId, out var error))
        {
            Log.Error(error);
        }
    }
}

public class CrossModAction(ProxyManager<Nothing> proxyManager, IModInfo mod, string id, string? category, Func<string>? name, Func<string>? description, Action action, Dictionary<string, string>? customFields, object? customData) : ICrossModAction
{
    public IModInfo Mod { get; } = mod;
    public string Id { get; } = $"{mod.Manifest.UniqueID}_{id}";
    public string Category { get; } = category ?? "None";
    public Func<string> Name { get; } = name ?? (() => Registrar.QualifyMethodName(action.Method));
    public Func<string> Description { get; } = description ?? (() => "(No description provided.)");
    public Action Action { get; } = action;
    public Dictionary<string, string>? CustomFields { get; } = customFields;
    public object? RawCustomData { get; } = customData;

    public T? GetCustomData<T>() where T : class
    {
        return RawCustomData is not null ? proxyManager.ObtainProxy<T>(RawCustomData) : null;
    }

    public void PerformAction()
    {
        Action();
    }
}