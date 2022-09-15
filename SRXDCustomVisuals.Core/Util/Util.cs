using System.Globalization;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public static class Util {
    public static bool TryParseInt(string value, out int intVal) => int.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out intVal);
    
    public static bool TryParseFloat(string value, out float floatVal) => float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out floatVal);
    
    public static bool TryParseVector(string value, out Vector3 vector) {
        vector = default;

        string[] split = value.Split(',');

        if (split.Length is < 1 or > 3)
            return false;

        if (!TryParseFloat(split[0], out float x))
            return false;

        if (split.Length == 1) {
            vector = new Vector3(x, x, x);

            return true;
        }
        
        if (!TryParseFloat(split[1], out float y))
            return false;

        if (split.Length == 2) {
            vector = new Vector3(x, y, 0f);

            return true;
        }

        if (!TryParseFloat(split[2], out float z))
            return false;

        vector = new Vector3(x, y, z);

        return true;
    }
    
    public static bool TryParseColor(string value, out Color color) {
        color = default;

        string[] split = value.Split(',');

        if (split.Length is < 1 or > 4)
            return false;

        if (!TryParseFloat(split[0], out float r))
            return false;

        if (split.Length == 1) {
            color = new Color(r, r, r, 1f);

            return true;
        }
        
        if (split.Length == 2 || !TryParseFloat(split[1], out float g) || !TryParseFloat(split[2], out float b))
            return false;

        if (split.Length == 3) {
            color = new Color(r, g, b, 1f);

            return true;
        }

        if (!TryParseFloat(split[3], out float a))
            return false;

        color = new Color(r, g, b, a);

        return true;
    }
}