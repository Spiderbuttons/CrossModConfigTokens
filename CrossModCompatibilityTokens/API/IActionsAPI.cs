using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.API;

public partial interface ICrossModCompatibilityToolsAPI
{
    /// <summary>
    /// Try to register an Action with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="manifest">Your mod's Manifest.</param>
    /// <param name="id">The ID you want to give to the Action you are registering.</param>
    /// <param name="action">The Action you want to register.</param>
    /// <param name="customFields">Optional. A dictionary of custom fields you want to attach to the Action.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Action was registered successfully.</returns>
    bool TryRegisterAction(IManifest manifest, string id, Action action, Dictionary<string, object>? customFields, out string? error);
    
    /// <summary>
    /// Try to get an Action that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Action.</param>
    /// <param name="actionId">The ID of the Action you want to get.</param>
    /// <param name="action">A CrossModAction instance containing the Action you want to get along with its metadata, or <c>null</c> if the Action was not found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Action was found.</returns>
    /// <remarks>If no Action with the given <c>actionId</c> is found, this function will attempt to find an Action whose method name matches the <c>actionId</c> instead as a fallback.</remarks>
    bool TryGetActionFromMod(IModInfo mod, string actionId, [NotNullWhen(true)] out ICrossModAction? action, out string? error);

    /// <summary>
    /// Try to get all Actions that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Actions.</param>
    /// <param name="actions">A Dictionary, keyed by Action ID, of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if the mod has not registered any Actions.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Actions were found.</returns>
    bool TryGetActionsFromMod(IModInfo mod, [NotNullWhen(true)] out IDictionary<string, ICrossModAction>? actions, out string? error);

    /// <summary>
    /// Try to invoke an Action that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Action.</param>
    /// <param name="actionId">The ID of the Action you want to invoke.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Action was invoked successfully.</returns>
    /// <remarks>If no Action with the given <c>actionId</c> is found, this function will attempt to find and invoke an Action whose method name matches the <c>actionId</c> instead as a fallback.</remarks>
    bool TryInvokeActionFromMod(IModInfo mod, string actionId, out string? error);
    
    /* */
    
    /// <summary>
    /// Register an Action with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="manifest">Your mod's Manifest.</param>
    /// <param name="id">The ID you want to give to the Action you are registering.</param>
    /// <param name="action">The Action you want to register.</param>
    /// <param name="customFields">Optional. A dictionary of custom fields you want to attach to the Action.</param>
    void RegisterAction(IManifest manifest, string id, Action action, Dictionary<string, object>? customFields);
    
    /// <summary>
    /// Get an Action that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Action.</param>
    /// <param name="actionId">The ID of the Action you want to get.</param>
    /// <returns>A CrossModAction instance containing the Action you want to get along with its metadata, or <c>null</c> if the Action was not found.</returns>
    /// <remarks>If no Action with the given <c>actionId</c> is found, this function will attempt to find an Action whose method name matches the <c>actionId</c> instead as a fallback.</remarks>
    ICrossModAction? GetActionFromMod(IModInfo mod, string actionId);
    
    /// <summary>
    /// Get all Actions that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Actions.</param>
    /// <returns>A Dictionary, keyed by Action ID, of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if the mod has not registered any Actions.</returns>
    IDictionary<string, ICrossModAction>? GetActionsFromMod(IModInfo mod);
    
    /// <summary>
    /// Invoke an Action that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Action.</param>
    /// <param name="actionId">The ID of the Action you want to invoke.</param>
    /// <remarks>If no Action with the given <c>actionId</c> is found, this function will attempt to find and invoke an Action whose method name matches the <c>actionId</c> instead as a fallback.</remarks>
    void InvokeActionFromMod(IModInfo mod, string actionId);
}

/// <summary>
/// Contains an Action along with some metadata surrounding it.
/// </summary>
public interface ICrossModAction
{
    /// <summary>
    /// The IModInfo corresponding to the mod that registered this CrossModAction.
    /// </summary>
    public IModInfo Mod { get; }
    
    /// <summary>
    /// The ID of this registered action.
    /// </summary>
    public string Id { get; }
    
    /// <summary>
    /// The registered Action.
    /// </summary>
    public Action Action { get; }
    
    /// <summary>
    /// The custom fields attached to this action, or <c>null</c> if there aren't any.
    /// </summary>
    public Dictionary<string, object>? CustomFields { get; }
}