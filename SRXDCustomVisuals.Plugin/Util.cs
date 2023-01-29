using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SRXDCustomVisuals.Plugin; 

public static class Util {
    public static string AssemblyPath { get; } = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Plugin)).Location);

    public static void InsertSorted<T>(this List<T> list, T item) where T : IComparable<T> => list.Insert(list.GetInsertIndex(item), item);
    
    public static bool TryLoadAssembly(string fileName) {
        string path = Path.Combine(AssemblyPath, fileName);
        
        if (!File.Exists(path)) {
            Plugin.Logger.LogWarning($"{fileName} was not found. Some visuals elements may be missing controllers as a result.");

            return false;
        }

        try {
            Assembly.LoadFrom(path);

            return true;
        }
        catch {
            Plugin.Logger.LogWarning($"Failed to load {fileName}. Some visuals elements may be missing controllers as a result.");
            
            return false;
        }
    }

    public static int GetInsertIndex<T>(this IReadOnlyList<T> list, T item) where T : IComparable<T> {
        int index = BinarySearch();

        while (index < list.Count && item.CompareTo(list[index]) >= 0)
            index++;

        return index;

        int BinarySearch() {
            int start = 0;
            int end = list.Count - 1;

            while (start <= end) {
                int mid = (start + end) / 2;
                int comparison = item.CompareTo(list[mid]);

                if (comparison < 0)
                    end = mid - 1;
                else if (comparison > 0)
                    start = mid + 1;
                else
                    return mid;
            }

            return start;
        }
    }
}