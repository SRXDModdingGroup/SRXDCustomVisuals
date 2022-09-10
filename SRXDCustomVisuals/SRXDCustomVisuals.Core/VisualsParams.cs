using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsParams : IVisualsParams {
    internal static VisualsParams Empty { get; } = new();
    
    private Dictionary<string, Vector4> values = new();

    public void SetBool(string key, bool val) => values[key] = new Vector4(val ? 1f : 0f, 0f, 0f, 0f);
    
    public void SetInt(string key, int val) => values[key] = new Vector4(val, 0f, 0f, 0f);

    public void SetFloat(string key, float val) => values[key] = new Vector4(val, 0f, 0f, 0f);

    public void SetVector(string key, Vector3 val) => values[key] = val;
    
    public void SetColor(string key, Color val) => values[key] = val;

    public void Clear() => values.Clear();

    public bool GetBool(string key) => GetBool(key, false);

    public bool GetBool(string key, bool defaultValue) => values.TryGetValue(key, out var value) ? value.x > 0f : defaultValue;

    public int GetInt(string key) => GetInt(key, 0);

    public int GetInt(string key, int defaultValue) => values.TryGetValue(key, out var value) ? Mathf.RoundToInt(value.x) : defaultValue;

    public float GetFloat(string key) => GetFloat(key, 0f);

    public float GetFloat(string key, float defaultValue) => values.TryGetValue(key, out var value) ? value.x : defaultValue;

    public Vector3 GetVector(string key) => GetVector(key, Vector3.zero);

    public Vector3 GetVector(string key, Vector3 defaultValue) => values.TryGetValue(key, out var value) ? value : defaultValue;

    public Color GetColor(string key) => GetColor(key, Color.white);

    public Color GetColor(string key, Color defaultValue) => values.TryGetValue(key, out var value) ? value : defaultValue;
}