using System;
using StardewModdingAPI;

namespace CrossModCompatibilityTools.API;

public interface IActionAPI
{
    void RegisterAction(IManifest mod, string id, Action action, object? customData = null);
    
    void RegisterAction(IManifest mod, string id, Func<string> name, Func<string> description, Action action, object? customData = null);
    
    ICrossModActionData? GetAction(string id);
}

public interface ICrossModActionData
{
    string Id { get; }
    
    IModInfo Mod { get; }

    Func<string?>Name { get; }
    
    Func<string?> Description { get; }
    
    Action Action { get; }
    
    T? GetCustomData<T>() where T : class;
    
    void PerformAction();
}