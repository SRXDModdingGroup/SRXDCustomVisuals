using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin; 

public static class Util {
    public static bool ColorEquals(Color32 a, Color32 b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;

    public static bool TryParseColor32(string hex, out Color32 color) {
        if (hex.Length == 6
            && byte.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r)
            && byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g)
            && byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b)) {
            color = new Color32(r, g, b, 255);

            return true;
        }

        color = default;

        return false;
    }
    
    public static int Mod(int a, int b) => (a % b + b) % b;

    public static int InsertSorted<T>(this List<T> list, T item) where T : IComparable<T> {
        int index = list.GetInsertIndex(item);
        
        list.Insert(index, item);

        return index;
    }
    
    public static string ToHexString(Color32 color) => $"{color.r:X2}{color.g:X2}{color.b:X2}";

    public static Color ToColor(this Color32 color) => new(color.r / 255f, color.g / 255f, color.b / 255f);

    public static T[] Copy<T>(this T[] array) {
        var newArray = new T[array.Length];
        
        Array.Copy(array, newArray, array.Length);

        return newArray;
    }

    private static int GetInsertIndex<T>(this IReadOnlyList<T> list, T item) where T : IComparable<T> {
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