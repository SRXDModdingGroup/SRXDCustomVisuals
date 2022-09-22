using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public interface IVisualsParams {
    VisualsValue GetValue(string key, VisualsValue defaultValue);
}

public static class VisualsParamsExtensions {
    public static bool GetBool(this IVisualsParams parameters, string key) => parameters.GetBool(key, false);
    public static bool GetBool(this IVisualsParams parameters, string key, bool defaultValue) => parameters.GetValue(key, new VisualsValue(defaultValue)).Bool;
    
    public static int GetInt(this IVisualsParams parameters, string key) => parameters.GetInt(key, 0);
    public static int GetInt(this IVisualsParams parameters, string key, int defaultValue) => parameters.GetValue(key, new VisualsValue(defaultValue)).Int;
    
    public static float GetFloat(this IVisualsParams parameters, string key) => parameters.GetFloat(key, 0f);
    public static float GetFloat(this IVisualsParams parameters, string key, float defaultValue) => parameters.GetValue(key, new VisualsValue(defaultValue)).Float;
    
    public static Vector3 GetVector(this IVisualsParams parameters, string key) => parameters.GetVector(key, Vector3.zero);
    public static Vector3 GetVector(this IVisualsParams parameters, string key, Vector3 defaultValue) => parameters.GetValue(key, new VisualsValue(defaultValue)).Vector;
    
    public static Color GetColor(this IVisualsParams parameters, string key) => parameters.GetColor(key, Color.white);
    public static Color GetColor(this IVisualsParams parameters, string key, Color defaultValue) => parameters.GetValue(key, new VisualsValue(defaultValue)).Color;
}