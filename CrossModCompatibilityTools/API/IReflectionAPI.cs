using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using StardewModdingAPI;

namespace CrossModCompatibilityTools.API;

public partial interface ICrossModCompatibilityToolsAPI
{
    /// <summary>
    /// Get the ModEntry instance of a loaded mod.
    /// </summary>
    /// <param name="mod">The mod to get the ModEntry instance of.</param>
    /// <param name="modInstance">The ModEntry instance of the mod.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the mod's ModEntry instance was found.</returns>
    bool TryGetModEntry(IModInfo mod, [NotNullWhen(true)] out IMod? modInstance, [NotNullWhen(false)] out string? error);

    /// <summary>
    /// Get a loaded Content Pack.
    /// </summary>
    /// <param name="mod">The mod to get the content pack for.</param>
    /// <param name="pack">The content pack for the mod.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the content pack was found.</returns>
    /// <remarks>You can grab any content pack, not just ones that belong to your mod.</remarks>
    bool TryGetContentPack(IModInfo mod, [NotNullWhen(true)] out IContentPack? pack, [NotNullWhen(false)] out string? error);
    
    /// <summary>
    /// Get the IModHelper for a loaded mod.
    /// </summary>
    /// <param name="mod">The mod to get the helper for.</param>
    /// <param name="helper">The IModHelper for the mod.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the mod's IModHelper was found.</returns>
    bool TryGetModHelper(IModInfo mod, [NotNullWhen(true)] out IModHelper? helper, [NotNullWhen(false)] out string? error);
    
    /// <summary>
    /// Get the Assembly of a loaded mod.
    /// </summary>
    /// <param name="mod">The mod to get the assembly for.</param>
    /// <param name="assembly">The Assembly of the mod.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the mod's Assembly was found.</returns>
    bool TryGetModAssembly(IModInfo mod, [NotNullWhen(true)] out Assembly? assembly, [NotNullWhen(false)] out string? error);

    /// <summary>
    /// Search for a Type, scoped to a mod's assembly.
    /// </summary>
    /// <param name="mod">The mod to get the type from.</param>
    /// <param name="typeName">The name of the type to search for.</param>
    /// <param name="type">The Type, if found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>>A bool indicating whether the type was found.</returns>
    /// <remarks><c>typeName</c> does not need to be a fully qualified name, but it might help if it is.</remarks>
    bool TryGetTypeFromMod(IModInfo mod, string typeName, [NotNullWhen(true)] out Type? type, [NotNullWhen(false)] out string? error);

    /// <summary>
    /// Search for a Method, scoped to a mod's assembly.
    /// </summary>
    /// <param name="mod">The mod to get the method from.</param>
    /// <param name="typeName">Optional. The name of the Type/class the method is in. Helps narrow down the search if provided.</param>
    /// <param name="methodName">The name of the method to search for.</param>
    /// <param name="method">The MethodInfo for the method, if found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>>A bool indicating whether the method was found.</returns>
    bool TryGetMethodFromMod(IModInfo mod, string? typeName, string methodName, [NotNullWhen(true)] out MethodInfo? method, [NotNullWhen(false)] out string? error);
    
    /// <summary>
    /// Get the ModEntry instance of a loaded mod.
    /// </summary>
    /// <param name="mod">The mod to get the ModEntry instance of.</param>
    /// <returns>The ModEntry instance of the mod, or <c>null</c> if the ModEntry was not found.</returns>
    IMod? GetModEntry(IModInfo mod);
    
    /// <summary>
    /// Get a loaded Content Pack.
    /// </summary>
    /// <param name="mod">The mod to get the content pack for.</param>
    /// <returns>The content pack for the mod, or <c>null</c> if the content pack was not found.</returns>
    IContentPack? GetContentPack(IModInfo mod);
    
    /// <summary>
    /// Get the IModHelper for a loaded mod.
    /// </summary>
    /// <param name="mod">The mod to get the helper for.</param>
    /// <returns>The IModHelper for the mod, or <c>null</c> if the IModHelper was not found.</returns>
    IModHelper? GetModHelper(IModInfo mod);
    
    /// <summary>
    /// Get the Assembly of a loaded mod.
    /// </summary>
    /// <param name="mod">The mod to get the assembly for.</param>
    /// <returns>The Assembly of the mod, or <c>null</c> if the Assembly was not found.</returns>
    Assembly? GetModAssembly(IModInfo mod);
    
    /// <summary>
    /// Search for a Type, scoped to a mod's assembly.
    /// </summary>
    /// <param name="mod">The mod to get the type from.</param>
    /// <param name="typeName">The name of the type to search for.</param>
    /// <returns>The Type from the mod, or <c>null</c> if the Type was not found.</returns>
    Type? GetTypeFromMod(IModInfo mod, string typeName);
    
    /// <summary>
    /// Search for a Method, scoped to a mod's assembly.
    /// </summary>
    /// <param name="mod">The mod to get the method from.</param>
    /// <param name="typeName">Optional. The name of the Type/class the method is in. Helps narrow down the search if provided.</param>
    /// <param name="methodName">The name of the method to search for.</param>
    /// <returns>The MethodInfo for the method, or <c>null</c> if the MethodInfo was not found.</returns>
    MethodInfo? GetMethodFromMod(IModInfo mod, string? typeName, string methodName);
}