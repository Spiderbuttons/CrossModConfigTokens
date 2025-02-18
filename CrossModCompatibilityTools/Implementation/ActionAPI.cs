using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CrossModCompatibilityTools.API;
using Nanoray.Pintail;
using StardewModdingAPI;

namespace CrossModCompatibilityTools.Implementation;

public partial class ActionAPI : IActionAPI
{
    private readonly ProxyManager<Nothing> proxyManager = CreateProxyManager();
    private readonly Dictionary<string, ICrossModActionData> CrossModActions = [];

    public void RegisterAction(IManifest mod, string id, Func<string> name, Func<string> description, Action action, object? customData = null)
    {
        var actionData = new CrossModActionData(proxyManager, mod, id, name, description, action, customData);
        CrossModActions[id] = actionData;
    }
    
    public void RegisterAction(IManifest mod, string id, Action action, object? customData = null)
    {
        var actionData = new CrossModActionData(proxyManager, mod, id, null, null, action, customData);
        CrossModActions[id] = actionData;
    }
    
    public ICrossModActionData? GetAction(string id)
    {
        return CrossModActions.GetValueOrDefault(id);
    }

    private static ProxyManager<Nothing> CreateProxyManager()
    {
        var assemblyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(new($"CrossModAPI.Proxies, Version=1.0.0.0, Culture=neutral"),
                AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("Proxies");
        return new ProxyManager<Nothing>(moduleBuilder,
            new ProxyManagerConfiguration<Nothing> { AccessLevelChecking = AccessLevelChecking.DisabledButOnlyAllowPublicMembers });
    }

    private class CrossModActionData(
        ProxyManager<Nothing> proxyManager,
        IManifest mod,
        string id,
        Func<string>? name,
        Func<string>? description,
        Action action,
        object? customData) : ICrossModActionData
    {
        public string Id { get; } = id;
        
        public IModInfo Mod { get; } = ModEntry.ModHelper.ModRegistry.Get(mod.UniqueID)!;
        
        public Func<string>? Name { get; } = name;
        
        public Func<string>? Description { get; } = description;
        
        public Action Action { get; } = action;

        public T? GetCustomData<T>() where T : class
        {
            return customData is not null ? proxyManager.ObtainProxy<T>(customData) : null;
        }

        public void PerformAction()
        {
            Action();
        }
    }
}