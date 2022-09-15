using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsParams : IVisualsParams {
    private Dictionary<string, Vector4> values = new();

    public void SetBool(string key, bool value) => SetFloat(key, value ? 1f : 0f);

    public void SetInt(string key, int value) => SetFloat(key, value);

    public void SetFloat(string key, float value) => values[key] = new Vector4(value, value, value, 1f);

    public void SetVector(string key, Vector3 value) => values[key] = new Vector4(value.x, value.y, value.z, 1f);
    
    public void SetColor(string key, Color value) => values[key] = value;

    public void Clear() => values.Clear();

    public bool GetBool(string key, bool defaultValue) => values.TryGetValue(key, out var value) ? value.x > 0f : defaultValue;

    public int GetInt(string key, int defaultValue) => values.TryGetValue(key, out var value) ? Mathf.RoundToInt(value.x) : defaultValue;

    public float GetFloat(string key, float defaultValue) => values.TryGetValue(key, out var value) ? value.x : defaultValue;

    public Vector3 GetVector(string key, Vector3 defaultValue) => values.TryGetValue(key, out var value) ? value : defaultValue;

    public Color GetColor(string key, Color defaultValue) => values.TryGetValue(key, out var value) ? value : defaultValue;
}