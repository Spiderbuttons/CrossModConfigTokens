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
    /// <param name="manifest">The manifest of the mod you want to register the Action for.</param>
    /// <param name="consumer">The UniqueID of the mod you intend to use this action, or <c>null</c> if you have no specific intentions.</param>
    /// <param name="actionId">The ID you want to give to the Action you are registering.</param>
    /// <param name="action">The Action you want to register.</param>
    /// <param name="customFields">Optional. A dictionary of custom fields you want to attach to the Action.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Action was registered successfully.</returns>
    bool TryRegisterAction(IManifest manifest, string? consumer, string actionId, Action action, Dictionary<string, object>? customFields, out string? error);
    
    /// <summary>
    /// Try to get an Action that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Action.</param>
    /// <param name="actionId">The ID of the Action you want to get.</param>
    /// <param name="action">A CrossModAction instance containing the Action you want to get along with its metadata, or <c>null</c> if the Action was not found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <param name="reflectIfNecessary">Optional. A bool indicating whether to use reflection to attempt to find the Action if it is not registered.</param>
    /// <returns>A bool indicating whether the Action was found.</returns>
    /// <remarks>
    /// <para>
    /// If no Action with the given <c>actionId</c> is found, this function will attempt to find a registered Action whose fully qualified method name matches the <c>actionId</c> instead as a fallback. If <c>reflectIfNecessary</c> is true, this function will reflect into the mod's assembly to find a method with a matching fully qualified name if it is otherwise unable to find the Action.
    /// </para>
    /// </remarks>
    bool TryGetActionFromMod(IModInfo mod, string actionId, [NotNullWhen(true)] out ICrossModAction? action, out string? error, bool reflectIfNecessary = false);

    /// <summary>
    /// Try to get all Actions that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Actions.</param>
    /// <param name="actions">A Dictionary, keyed by Action ID, of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if the mod has not registered any Actions.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Actions were found.</returns>
    bool TryGetActionsFromMod(IModInfo mod, [NotNullWhen(true)] out IDictionary<string, ICrossModAction>? actions, out string? error);

    /// <summary>
    /// Try to get all Actions registered by all mods that are intended for a specific consumer.
    /// </summary>
    /// <param name="consumer">The manifest of the mod who the Actions are intended for.</param>
    /// <param name="actions">A List of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if no Actions were found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Actions were found.</returns>
    bool TryGetActionsForConsumer(IManifest consumer, [NotNullWhen(true)] out List<ICrossModAction>? actions, out string? error);

    /// <summary>
    /// Try to invoke an Action that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Action.</param>
    /// <param name="actionId">The ID of the Action you want to invoke.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <param name="reflectIfNecessary">Optional. A bool indicating whether to use reflection to attempt to find and invoke the Action if it is not registered.</param>
    /// <returns>A bool indicating whether the Action was invoked successfully.</returns>
    /// <remarks>
    /// <para>
    /// If no Action with the given <c>actionId</c> is found, this function will attempt to find and invoke a registered Action whose fully qualified method name matches the <c>actionId</c> instead as a fallback. If <c>reflectIfNecessary</c> is true, this function will reflect into the mod's assembly to find and invoke a method with a matching fully qualified name if it is otherwise unable to find the Action.
    /// </para>
    /// </remarks>
    bool TryInvokeActionFromMod(IModInfo mod, string actionId, out string? error, bool reflectIfNecessary = false);

    /// <summary>
    /// Register an Action with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="manifest">The manifest of the mod you want to register the Action for.</param>
    /// <param name="consumer">The UniqueID of the mod you intend to use this action, or <c>null</c> if you have no specific intentions.</param>
    /// <param name="actionId">The ID you want to give to the Action you are registering.</param>
    /// <param name="action">The Action you want to register.</param>
    /// <param name="customFields">Optional. A dictionary of custom fields you want to attach to the Action.</param>
    void RegisterAction(IManifest manifest, string? consumer, string actionId, Action action, Dictionary<string, object>? customFields);
    
    /// <summary>
    /// Get an Action that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Action.</param>
    /// <param name="actionId">The ID of the Action you want to get.</param>
    /// <param name="reflectIfNecessary">Optional. A bool indicating whether to use reflection to attempt to find the Action if it is not registered.</param>
    /// <returns>A CrossModAction instance containing the Action you want to get along with its metadata, or <c>null</c> if the Action was not found.</returns>
    /// <remarks>
    /// <para>
    /// If no Action with the given <c>actionId</c> is found, this function will attempt to find a registered Action whose fully qualified method name matches the <c>actionId</c> instead as a fallback. If <c>reflectIfNecessary</c> is true, this function will reflect into the mod's assembly to find a method with a matching fully qualified name if it is otherwise unable to find the Action.
    /// </para>
    /// </remarks>
    ICrossModAction? GetActionFromMod(IModInfo mod, string actionId, bool reflectIfNecessary = false);
    
    /// <summary>
    /// Get all Actions that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Actions.</param>
    /// <returns>A Dictionary, keyed by Action ID, of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if the mod has not registered any Actions.</returns>
    IDictionary<string, ICrossModAction>? GetActionsFromMod(IModInfo mod);
    
    /// <summary>
    /// Get all Actions registered by all mods that are intended for a specific consumer.
    /// </summary>
    /// <param name="consumer">The manifest of the mod who the Actions are intended for.</param>
    /// <returns>A List of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if no Actions were found.</returns>    
    List<ICrossModAction>? GetActionsForConsumer(IManifest consumer);
    
    /// <summary>
    /// Invoke an Action that another mod registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="mod">The mod that registered the Action.</param>
    /// <param name="actionId">The ID of the Action you want to invoke.</param>
    /// <param name="reflectIfNecessary">Optional. A bool indicating whether to use reflection to attempt to find and invoke the Action if it is not registered.</param>
    /// <remarks>
    /// <para>
    /// If no Action with the given <c>actionId</c> is found, this function will attempt to find and invoke a registered Action whose fully qualified method name matches the <c>actionId</c> instead as a fallback. If <c>reflectIfNecessary</c> is true, this function will reflect into the mod's assembly to find and invoke a method with a matching fully qualified name if it is otherwise unable to find the Action.
    /// </para>
    /// </remarks>
    void InvokeActionFromMod(IModInfo mod, string actionId, bool reflectIfNecessary = false);
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
    /// The UniqueID of the intended consumer of this action (e.g. if you are registering this action so that Mod A can use it, write Mod A's unique ID) 
    /// </summary>
    public string? IntendedConsumer { get; }
    
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