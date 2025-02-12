using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.API;

public partial interface ICrossModCompatibilityToolsAPI
{
    /// <summary>
    /// Try to get the value(s) of a Content Patcher content pack's dynamic token.
    /// </summary>
    /// <param name="mod">The mod whose dynamic token you want to get.</param>
    /// <param name="token">The name of the dynamic token you want to get the value(s) of.</param>
    /// <param name="values">The value(s) of the dynamic token, or <c>null</c> if it isn't found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the dynamic token was successfully found.</returns>
    bool TryGetDynamicTokenValues(IModInfo mod, string token, [NotNullWhen(true)] out IEnumerable<string>? values, out string? error);
    
    /// <summary>
    /// Get the value(s) of a Content Patcher content pack's dynamic token.
    /// </summary>
    /// <param name="mod">The mod whose dynamic token you want to get.</param>
    /// <param name="token">The name of the dynamic token you want to get the value(s) of.</param>
    /// <returns>The value(s) of the dynamic token, or <c>null</c> if it isn't found.</returns>
    IEnumerable<string>? GetDynamicTokenValues(IModInfo mod, string token);
}