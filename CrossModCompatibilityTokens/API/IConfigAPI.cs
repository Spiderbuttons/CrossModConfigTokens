using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace CrossModCompatibilityTokens.API;

public partial interface ICrossModCompatibilityToolsAPI
{
    bool TryGetConfigValue(IModInfo mod, string configKey, [NotNullWhen(true)] out string? configValue, out string? error);
    
    bool TryGetConfigValue<T>(IModInfo mod, string configKey, [NotNullWhen(true)] out T? configValue, out string? error);

    bool TryGetConfig(IModInfo mod, [NotNullWhen(true)] out Dictionary<string, object>? configObject, out string? error);

    bool TryGetConfig(IModInfo mod, [NotNullWhen(true)] out JObject? configObject, out string? error);
    
    /* */
    
    string? GetConfigValue(IModInfo mod, string configKey);
    
    T? GetConfigValue<T>(IModInfo mod, string configKey);
    
    Dictionary<string, object>? GetConfig(IModInfo mod);
    
    JObject? GetConfigJObject(IModInfo mod);
}