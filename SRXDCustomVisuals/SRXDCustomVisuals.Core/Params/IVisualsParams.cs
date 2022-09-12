using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public interface IVisualsParams {
    bool GetBool(string key, bool defaultValue);
    
    int GetInt(string key, int defaultValue);
    
    float GetFloat(string key, float defaultValue);
    
    Vector3 GetVector(string key, Vector3 defaultValue);
    
    Color GetColor(string key, Color defaultValue);
}

public static class VisualsParamsExtensions {
    public static bool GetBool(this IVisualsParams parameters, string key) => parameters.GetBool(key, false);
    
    public static int GetInt(this IVisualsParams parameters, string key) => parameters.GetInt(key, 0);
    
    public static float GetFloat(this IVisualsParams parameters, string key) => parameters.GetFloat(key, 0f);
    
    public static Vector3 GetVector(this IVisualsParams parameters, string key) => parameters.GetVector(key, Vector3.zero);
    
    public static Color GetColor(this IVisualsParams parameters, string key) => parameters.GetColor(key, Color.white);
}