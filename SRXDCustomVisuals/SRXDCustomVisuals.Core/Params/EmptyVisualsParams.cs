using UnityEngine;

namespace SRXDCustomVisuals.Core; 

internal class EmptyVisualsParams : IVisualsParams {
    public static EmptyVisualsParams Instance { get; } = new();
    
    public bool GetBool(string key, bool defaultValue) => defaultValue;

    public int GetInt(string key, int defaultValue) => defaultValue;

    public float GetFloat(string key, float defaultValue) => defaultValue;

    public Vector3 GetVector(string key, Vector3 defaultValue) => defaultValue;

    public Color GetColor(string key, Color defaultValue) => defaultValue;
}