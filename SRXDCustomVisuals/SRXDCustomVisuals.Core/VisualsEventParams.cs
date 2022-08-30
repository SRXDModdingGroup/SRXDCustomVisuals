using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsEventParams {
    internal static VisualsEventParams Empty { get; } = new();
    
    private Dictionary<string, int> intVals = new();
    private Dictionary<string, float> floatVals = new();
    private Dictionary<string, Vector4> vectorVals = new();
    
    public void SetInt(string key, int val) => intVals[key] = val;

    public void SetFloat(string key, float val) => floatVals[key] = val;

    public void SetVector(string key, Vector3 val) => vectorVals[key] = val;
    
    public void SetColor(string key, Color val) => vectorVals[key] = val;

    public void Clear() {
        intVals.Clear();
        floatVals.Clear();
        vectorVals.Clear();
    }

    public int GetInt(string key) {
        if (intVals.TryGetValue(key, out int value))
            return value;

        return 0;
    }

    public float GetFloat(string key) {
        if (floatVals.TryGetValue(key, out float value))
            return value;

        return 0f;
    }

    public Vector3 GetVector(string key) {
        if (vectorVals.TryGetValue(key, out var value))
            return value;

        return Vector3.zero;
    }

    public Color GetColor(string key) {
        if (vectorVals.TryGetValue(key, out var value))
            return value;

        return Color.white;
    }
}