using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Logger
{
    public static void Log(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        Debug.Log($"[{System.IO.Path.GetFileName(filePath)}:{lineNumber}] {memberName}() → {message}");
    }
}