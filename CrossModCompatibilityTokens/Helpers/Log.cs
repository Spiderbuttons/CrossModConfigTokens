using System.Collections.Generic;
using StardewModdingAPI;
using System.Linq;
using HarmonyLib;

namespace CrossModCompatibilityTokens.Helpers;

public static class Log
{
    public static void Debug<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message.GetType() + "] " : string.Empty)}{message.ToString() ?? string.Empty}",
        LogLevel.Debug);

    public static void Error<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message.GetType() + "] " : string.Empty)}{message.ToString() ?? string.Empty}",
        LogLevel.Error);

    public static void Warn<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message.GetType() + "] " : string.Empty)}{message.ToString() ?? string.Empty}",
        LogLevel.Warn);

    public static void Info<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message.GetType() + "] " : string.Empty)}{message.ToString() ?? string.Empty}",
        LogLevel.Info);

    public static void Trace<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message.GetType() + "] " : string.Empty)}{message.ToString() ?? string.Empty}",
        LogLevel.Trace);

    public static void Alert<T>(T message) => ModEntry.ModMonitor.Log(
        $"{(message is not string ? "[" + message.GetType() + "] " : string.Empty)}{message.ToString() ?? string.Empty}",
        LogLevel.Alert);

    public static void ILCode(IEnumerable<CodeInstruction> newCode, IEnumerable<CodeInstruction> originalCode)
    {
        var originalEnumerator = 0;
        foreach (var instruction in newCode)
        {
            if (originalEnumerator >= originalCode.Count())
            {
                Warn($"{instruction.opcode} {instruction.operand}");
                continue;
            }

            if (instruction.opcode != originalCode.ElementAt(originalEnumerator).opcode ||
                instruction.operand != originalCode.ElementAt(originalEnumerator).operand)
            {
                Warn($"{instruction.opcode} {instruction.operand}");
                continue;
            }

            Debug($"{instruction.opcode} {instruction.operand}");
            originalEnumerator++;
        }
    }

    public static void ILCode(CodeInstruction code)
    {
        Debug($"{code.opcode} {code.operand}");
    }
}