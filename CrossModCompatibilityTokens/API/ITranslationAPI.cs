using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.API;

public partial interface ICrossModCompatibilityToolsAPI
{
    /// <summary>
    /// Try to get a translation from another mod's i18n.
    /// </summary>
    /// <param name="mod">The mod whose i18n you want to look in.</param>
    /// <param name="transKey">The i18n key of the translation you want to get.</param>
    /// <param name="transValue">The translation, or <c>null</c> if it isn't found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the translation was successfully found.</returns>
    bool TryGetTranslation(IModInfo mod, string transKey, [NotNullWhen(true)] out Translation? transValue, out string? error);
    
    /// <summary>
    /// Try to get a translation from another mod's i18n.
    /// </summary>
    /// <param name="mod">The mod whose i18n you want to look in.</param>
    /// <param name="transKey">The i18n key of the translation you want to get.</param>
    /// <param name="tokens">The tokens to use for the translation, or <c>null</c> if you don't want to use any. <see href="https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Translation#Built-in_API"/></param>
    /// <param name="transValue"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    bool TryGetTranslation(IModInfo mod, string transKey, object? tokens, [NotNullWhen(true)] out Translation? transValue, out string? error);
    
    /// <summary>
    /// Try to get the translation helper from another mod.
    /// </summary>
    /// <param name="mod">The mod whose translation helper you want to get.</param>
    /// <param name="translator">The translation helper, or <c>null</c> if it isn't found.</param>
    /// <param name="error">The error indicating what went wrong, or <c>null</c> if everything went right.</param>
    /// <returns>A bool indicating whether the translation helper was successfully found.</returns>
    bool TryGetTranslationHelper(IModInfo mod, [NotNullWhen(true)] out ITranslationHelper? translator, out string? error);
    
    /// <summary>
    /// Get a translation from another mod's i18n.
    /// </summary>
    /// <param name="mod">The mod whose i18n you want to look in.</param>
    /// <param name="transKey">The i18n key of the translation you want to get.</param>
    /// <returns>The translation, or <c>null</c> if it isn't found.</returns>
    Translation? GetTranslation(IModInfo mod, string transKey);
    
    /// <summary>
    /// Get a translation from another mod's i18n.
    /// </summary>
    /// <param name="mod">The mod whose i18n you want to look in.</param>
    /// <param name="transKey">The i18n key of the translation you want to get.</param>
    /// <param name="tokens">The tokens to use for the translation, or <c>null</c> if you don't want to use any. <see href="https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Translation#Built-in_API"/></param>
    /// <returns>The translation, or <c>null</c> if it isn't found.</returns>
    Translation? GetTranslation(IModInfo mod, string transKey, object? tokens);
    
    /// <summary>
    /// Get the translation helper from another mod.
    /// </summary>
    /// <param name="mod">The mod whose translation helper you want to get.</param>
    /// <returns>The translation helper, or <c>null</c> if it isn't found.</returns>
    ITranslationHelper? GetTranslationHelper(IModInfo mod);
}