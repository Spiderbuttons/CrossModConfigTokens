using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.API;

public partial interface ICrossModCompatibilityToolsAPI
{
    bool TryRegisterAction(IManifest mod, string id, Action action, Dictionary<string, object>? customFields, out string? error);
    
    bool TryGetActionFromMod(IModInfo mod, string actionId, [NotNullWhen(true)] out ICrossModAction? action, out string? error);

    bool TryGetActionsFromMod(IModInfo mod, [NotNullWhen(true)] out IDictionary<string, ICrossModAction>? actions, out string? error);

    bool TryInvokeActionFromMod(IModInfo mod, string actionId, out string? error);
}

public interface ICrossModAction
{
    public IModInfo Mod { get; }
    public string Id { get; }
    public Action Action { get; }
    public Dictionary<string, object> CustomFields { get; }
}