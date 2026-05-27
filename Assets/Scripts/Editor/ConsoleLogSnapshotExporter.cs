using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ConsoleLogSnapshotExporter
{
    private const string OutputPath = "Logs/UnityConsoleSnapshot.txt";

    static ConsoleLogSnapshotExporter()
    {
        EditorApplication.delayCall -= ExportSnapshot;
        EditorApplication.delayCall += ExportSnapshot;
    }

    [MenuItem("Tools/Diagnostics/Dump Console Snapshot")]
    public static void ExportSnapshot()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

            Type logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            Type logEntryType = Type.GetType("UnityEditor.LogEntry,UnityEditor.dll");

            if (logEntriesType == null || logEntryType == null)
            {
                File.WriteAllText(OutputPath, "Could not resolve UnityEditor.LogEntries or UnityEditor.LogEntry.");
                return;
            }

            MethodInfo getCount = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo startGettingEntries = logEntriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo getEntryInternal = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo endGettingEntries = logEntriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (getCount == null || getEntryInternal == null)
            {
                File.WriteAllText(OutputPath, "Could not resolve required LogEntries methods.");
                return;
            }

            int count = Convert.ToInt32(getCount.Invoke(null, null));
            int startIndex = Mathf.Max(0, count - 200);
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Unity Console Snapshot");
            builder.AppendLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            builder.AppendLine("Entry Count: " + count);
            builder.AppendLine();

            startGettingEntries?.Invoke(null, null);

            for (int i = startIndex; i < count; i++)
            {
                object entry = Activator.CreateInstance(logEntryType);
                object result = getEntryInternal.Invoke(null, new[] { (object)i, entry });

                if (result is bool hasEntry && !hasEntry)
                    continue;

                builder.AppendLine("[" + i + "]");
                AppendValue(builder, logEntryType, entry, "mode", "Mode");
                AppendValue(builder, logEntryType, entry, "condition", "Message");
                AppendValue(builder, logEntryType, entry, "message", "Message");
                AppendValue(builder, logEntryType, entry, "file", "File");
                AppendValue(builder, logEntryType, entry, "line", "Line");
                AppendValue(builder, logEntryType, entry, "callstackText", "Stack");
                AppendValue(builder, logEntryType, entry, "stackTrace", "Stack");
                builder.AppendLine();
            }

            endGettingEntries?.Invoke(null, null);
            File.WriteAllText(OutputPath, builder.ToString());
        }
        catch (Exception exception)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            File.WriteAllText(OutputPath, exception.ToString());
        }
    }

    private static void AppendValue(StringBuilder builder, Type entryType, object entry, string memberName, string label)
    {
        FieldInfo field = entryType.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field != null)
        {
            object value = field.GetValue(entry);
            AppendIfUseful(builder, label, value);
            return;
        }

        PropertyInfo property = entryType.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property != null)
        {
            object value = property.GetValue(entry);
            AppendIfUseful(builder, label, value);
        }
    }

    private static void AppendIfUseful(StringBuilder builder, string label, object value)
    {
        if (value == null)
            return;

        string text = value.ToString();

        if (string.IsNullOrWhiteSpace(text))
            return;

        builder.AppendLine(label + ": " + text);
    }
}
