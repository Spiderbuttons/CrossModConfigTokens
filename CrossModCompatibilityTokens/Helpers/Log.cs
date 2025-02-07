using System.Collections.Generic;
using StardewModdingAPI;
using System.Linq;

namespace CrossModCompatibilityTokens.Helpers;

public static class Log
{
    public static void Debug<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Debug);

    public static void Error<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Error);

    public static void Warn<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Warn);

    public static void Info<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Info);

    public static void Trace<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Trace);

    public static void Alert<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Alert);
}