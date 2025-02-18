using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace CrossModCompatibilityTools.API;

public partial interface ICrossModCompatibilityToolsAPI
{
    /// <summary>
    /// Try to register an Action with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="manifest">The manifest of the mod you want to register the Action for.</param>
    /// <param name="actionId">The ID you want to give to the Action you are registering.</param>
    /// <param name="category">Optional. A user-defined category that this action belongs to. Arbitrary and up to you, but if you expect a certain mod to use your action, you may want to check if they expect any particular category. Defaults to <c>Default</c></param>
    /// <param name="description">Optional. A description explaining what this action is meant to do or how it is meant to be used. Defaults to the name of the MethodInfo of the Action.</param>
    /// <param name="action">The Action you want to register.</param>
    /// <param name="customData">Optional. A class object of custom data you want to attach to the Action. You should explain your class structure in your documentation if you want others to use this custom data.</param>
    /// <param name="customFields">Optional. A dictionary of custom fields you want to attach to the Action.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Action was registered successfully.</returns>
    /// <remarks>All Action IDs are prefixed with the UniqueID found in <c>manifest</c>.</remarks>
    bool TryRegisterAction(IManifest manifest, string actionId, string? category, Func<string>? description, Action action, Dictionary<string, string>? customFields, object? customData, [NotNullWhen(false)] out string? error);
    
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
    /// Try to get all Actions that another mod registered with Cross-Mod Compatibility Tools as a dictionary.
    /// </summary>
    /// <param name="mod">The mod that registered the Actions.</param>
    /// <param name="actions">A Dictionary, keyed by Action ID, of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if the mod has not registered any Actions.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Actions were found.</returns>
    bool TryGetActionsFromModAsDict(IModInfo mod, [NotNullWhen(true)] out IReadOnlyDictionary<string, ICrossModAction>? actions, [NotNullWhen(false)] out string? error);

    /// <summary>
    /// Try to get all Actions that another mod registered with Cross-Mod Compatibility Tools as a list.
    /// </summary>
    /// <param name="mod">The mod that registered the Actions.</param>
    /// <param name="actions">A List of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if no Actions were found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Actions were found.</returns>
    bool TryGetActionsFromModAsList(IModInfo mod, [NotNullWhen(true)] out IReadOnlyList<ICrossModAction>? actions, [NotNullWhen(false)] out string? error);

    /// <summary>
    /// Try to get all Actions registered by all mods that have a specific category.
    /// </summary>
    /// <param name="category">The user-defined category that the Actions belong to.</param>
    /// <param name="actions">A List of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if no Actions were found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Actions were found.</returns>
    bool TryGetActionsByCategory(string category, [NotNullWhen(true)] out IReadOnlyList<ICrossModAction>? actions, [NotNullWhen(false)] out string? error);

    /// <summary>
    /// Try to get all Actions registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="actions">A Dictionary, keyed by Action ID, of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if no Actions were found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the Actions were found.</returns>
    bool TryGetActionsAsDict([NotNullWhen(true)] out IReadOnlyDictionary<string, ICrossModAction>? actions, [NotNullWhen(false)] out string? error);

    /// <summary>
    /// Try to get all Actions registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="actions">A List of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if no Actions were found.</param>
    /// <param name="error"></param>
    /// <returns>A bool indicating whether the Actions were found.</returns>
    bool TryGetActionsAsList([NotNullWhen(true)] out IReadOnlyList<ICrossModAction>? actions, [NotNullWhen(false)] out string? error);

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
    /// Try to invoke an Action registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="actionId">The ID of the Action you want to invoke.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>  
    /// <returns>A bool indicating whether the Action was invoked successfully.</returns>
    bool TryInvokeActionById(string actionId, [NotNullWhen(false)] out string? error);

    /// <summary>
    /// Register an Action with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="manifest">The manifest of the mod you want to register the Action for.</param>
    /// <param name="actionId">The ID you want to give to the Action you are registering.</param>
    /// <param name="category">Optional. A user-defined category that this action belongs to. Arbitrary and up to you, but if you expect a certain mod to use your action, you may want to check if they expect any particular category. Defaults to <c>Default</c></param>
    /// <param name="description">Optional. A description explaining what this action is meant to do or how it is meant to be used. Defaults to the name of the MethodInfo of the Action.</param>
    /// <param name="action">The Action you want to register.</param>
    /// <param name="customFields">Optional. A dictionary of custom fields you want to attach to the Action.</param>
    /// <param name="customData">Optional. A class object of custom data you want to attach to the Action. You should explain your class structure in your documentation if you want others to use this custom data.</param>
    void RegisterAction(IManifest manifest, string actionId, string? category, Func<string>? description, Action action, Dictionary<string, string>? customFields, object? customData);
    
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
    /// Get all Actions that another mod registered with Cross-Mod Compatibility Tools as a dictionary.
    /// </summary>
    /// <param name="mod">The mod that registered the Actions.</param>
    /// <returns>A Dictionary, keyed by Action ID, of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if the mod has not registered any Actions.</returns>
    IReadOnlyDictionary<string, ICrossModAction>? GetActionsFromModAsDict(IModInfo mod);
    
    /// <summary>
    /// Get all Actions that another mod registered with Cross-Mod Compatibility Tools as a list.
    /// </summary>
    /// <param name="mod">The mod that registered the Actions.</param>
    /// <returns>A List of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if no Actions were found.</returns>
    IReadOnlyList<ICrossModAction>? GetActionsFromModAsList(IModInfo mod);

    /// <summary>
    /// Get all Actions registered with Cross-Mod Compatibility Tools that have a specific category.
    /// </summary>
    /// <param name="category">A user-defined category that the Actions belong to.</param>
    /// <returns>A List of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if no Actions were found.</returns>
    IReadOnlyList<ICrossModAction>? GetActionsByCategory(string category);
    
    /// <summary>
    /// Get all Actions registered with Cross-Mod Compatibility Tools as a dictionary.
    /// </summary>
    /// <returns>A Dictionary, keyed by Action ID, of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if no Actions were found.</returns>
    IReadOnlyDictionary<string, ICrossModAction>? GetActionsAsDict();
    
    /// <summary>
    /// Get all Actions registered with Cross-Mod Compatibility Tools as a list.
    /// </summary>
    /// <returns>A List of CrossModAction instances containing the Actions you want to get along with their metadata, or <c>null</c> if no Actions were found.</returns>
    IReadOnlyList<ICrossModAction>? GetActionsAsList();
    
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

    /// <summary>
    /// Invoke an Action registered with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="actionId">The ID of the Action you want to invoke.</param>
    void InvokeActionById(string actionId);
}

/// <summary>
/// Contains an Action along with some metadata surrounding it. Copying this interface is required if you want to use any registered actions, but not if you only want to register your own.
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
    /// The user-defined category that this action belongs to. Arbitrary and up to you, but if you expect a certain mod to use your action, you may want to check if they expect any particular category. 
    /// </summary>
    /// <remarks>Defaults to <c>None</c>.</remarks>
    public string Category { get; }
    
    /// <summary>
    /// Optional. A description explaining what this action is meant to do or how it is meant to be used.
    /// </summary>
    public Func<string>? Description { get; }
    
    /// <summary>
    /// The registered Action.
    /// </summary>
    public Action Action { get; }
    
    /// <summary>
    /// The custom fields dictionary attached to this action, or <c>null</c> if there aren't any.
    /// </summary>
    public Dictionary<string, string>? CustomFields { get; }

    /// <summary>
    /// Get the custom data attached to this action, or <c>null</c> if there isn't any.
    /// </summary>
    /// <typeparam name="T">The interface type you want to parse this custom data as. You should check the documentation of the mod that produced this action for an interface to copy.</typeparam>
    /// <returns>The custom data attached to this action, or <c>null</c> if there isn't any.</returns>    
    public T? GetCustomData<T>() where T : class;

    /// <summary>
    /// Invoke the registered Action.
    /// </summary>
    public void PerformAction();
}