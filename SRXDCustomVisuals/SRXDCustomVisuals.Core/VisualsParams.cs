using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsParams : IVisualsParams {
    internal static VisualsParams Empty { get; } = new();
    
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

    public int GetInt(string key) => GetInt(key, 0);

    public int GetInt(string key, int defaultValue) => intVals.TryGetValue(key, out int value) ? value : defaultValue;

    public float GetFloat(string key) => GetFloat(key, 0f);

    public float GetFloat(string key, float defaultValue) => floatVals.TryGetValue(key, out float value) ? value : defaultValue;

    public Vector3 GetVector(string key) => GetVector(key, Vector3.zero);

    public Vector3 GetVector(string key, Vector3 defaultValue) => vectorVals.TryGetValue(key, out var value) ? value : defaultValue;

    public Color GetColor(string key) => GetColor(key, Color.white);

    public Color GetColor(string key, Color defaultValue) => vectorVals.TryGetValue(key, out var value) ? value : defaultValue;
}