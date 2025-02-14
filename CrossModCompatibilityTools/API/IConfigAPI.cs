using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace CrossModCompatibilityTools.API;

public partial interface ICrossModCompatibilityToolsAPI
{
    /// <summary>
    /// Try to get the value of a specific config option from a specific mod as a string.
    /// </summary>
    /// <param name="mod">The mod whose config you want to look at.</param>
    /// <param name="configKey">The name of the config option to look for.</param>
    /// <param name="configValue">The value read from the <c>config.json</c>, or <c>null</c> if it isn't found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the config value was successfully found.</returns>
    bool TryGetConfigValue(IModInfo mod, string configKey, [NotNullWhen(true)] out string? configValue, out string? error);
    
    /// <summary>
    /// Try to get the value of a specific config option from a specific mod as a specific type.
    /// </summary>
    /// <param name="mod">The mod whose config you want to look at.</param>
    /// <param name="configKey">The name of the config option to look for.</param>
    /// <param name="configValue">The value read from the <c>config.json</c> and parsed as the specified type, or <c>null</c> if it isn't found or cannot be parsed.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <typeparam name="T">The type to attempt to parse the config value as.</typeparam>
    /// <returns>A bool indicating whether the config value was successfully found and parsed.</returns>
    /// <remarks>
    ///     <para>Simple types like bools, ints, floats, etc. should parse fine, as well as any type with a <c>Parse(string)</c> or <c>TryParse(string, out object, out object)</c> method. Otherwise, you will need to use the non-generic version of this method to get the string value and parse it yourself.
    ///     </para>
    /// </remarks>
    bool TryGetConfigValue<T>(IModInfo mod, string configKey, [NotNullWhen(true)] out T? configValue, out string? error);

    /// <summary>
    /// Try to get the entire config object from a specific mod as a Dictionary of strings to objects.
    /// </summary>
    /// <param name="mod">The mod whose config you want to look at.</param>
    /// <param name="configObject">The config object read from the <c>config.json</c>, or <c>null</c> if it isn't found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the config object was successfully found.</returns>
    bool TryGetConfig(IModInfo mod, [NotNullWhen(true)] out Dictionary<string, object>? configObject, out string? error);

    /// <summary>
    /// Try to get the entire config object from a specific mod as a Newtonsoft.Json JObject.
    /// </summary>
    /// <param name="mod">The mod whose config you want to look at.</param>
    /// <param name="configObject">The config object read from the <c>config.json</c>, or <c>null</c> if it isn't found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the config object was successfully found.</returns>
    /// <remarks>You will need a reference to Newtonsoft.Json to use this method.</remarks>
    bool TryGetConfigJObject(IModInfo mod, [NotNullWhen(true)] out JObject? configObject, out string? error);

    /// <summary>
    /// Try to get the actual config class from a specific mod.
    /// </summary>
    /// <param name="mod">The mod whose config you want to look for.</param>
    /// <param name="configClass">The config class, or <c>null</c> if it isn't found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the config class was successfully found.</returns>
    /// <remarks>This will only look inside a mod's entry class for a field or property that holds their config class.</remarks>
    bool TryGetConfigClass(IModInfo mod, [NotNullWhen(true)] out object? configClass, out string? error);
    
    /* */
    
    /// <summary>
    /// Get the value of a specific config option from a specific mod as a string.
    /// </summary>
    /// <param name="mod">The mod whose config you want to look at.</param>
    /// <param name="configKey">The name of the config option to look for.</param>
    /// <returns>The value read from the <c>config.json</c>, or <c>null</c> if it isn't found.</returns>
    string? GetConfigValue(IModInfo mod, string configKey);
    
    /// <summary>
    /// Get the value of a specific config option from a specific mod as a specific type.
    /// </summary>
    /// <param name="mod">The mod whose config you want to look at.</param>
    /// <param name="configKey">The name of the config option to look for.</param>
    /// <typeparam name="T">The type to attempt to parse the config value as.</typeparam>
    /// <returns>The value read from the <c>config.json</c> and parsed as the specified type, or <c>null</c> if it isn't found or cannot be parsed.</returns>
    /// <remarks>
    ///     <para>Simple types like bools, ints, floats, etc. should parse fine, as well as any type with a <c>Parse(string)</c> or <c>TryParse(string, out object, out object)</c> method. Otherwise, you will need to use the non-generic version of this method to get the string value and parse it yourself.
    ///     </para>
    /// </remarks>
    T? GetConfigValue<T>(IModInfo mod, string configKey);
    
    /// <summary>
    /// Get the entire config object from a specific mod as a Dictionary of strings to objects.
    /// </summary>
    /// <param name="mod">The mod whose config you want to look at.</param>
    /// <returns>The config object read from the <c>config.json</c>, or <c>null</c> if it isn't found.</returns>
    Dictionary<string, object>? GetConfig(IModInfo mod);
    
    /// <summary>
    /// Get the entire config object from a specific mod as a Newtonsoft.Json JObject.
    /// </summary>
    /// <param name="mod">The mod whose config you want to look at.</param>
    /// <returns>The config object read from the <c>config.json</c>, or <c>null</c> if it isn't found.</returns>
    /// <remarks>You will need a reference to Newtonsoft.Json to use this method.</remarks>
    JObject? GetConfigJObject(IModInfo mod);
    
    /// <summary>
    /// Get the actual config class from a specific mod.
    /// </summary>
    /// <param name="mod">The mod whose config you want to look for.</param>
    /// <returns>The config class, or <c>null</c> if it isn't found.</returns>
    /// <remarks>This will only look inside a mod's entry class for a field or property that holds their config class.</remarks>
    object? GetConfigClass(IModInfo mod);
}