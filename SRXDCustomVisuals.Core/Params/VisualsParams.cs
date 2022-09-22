using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsParams : IVisualsParams {
    private Dictionary<string, VisualsValue> values = new();

    public void SetBool(string key, bool value) => SetValue(key, new VisualsValue(value));

    public void SetInt(string key, int value) => SetValue(key, new VisualsValue(value));

    public void SetFloat(string key, float value) => SetValue(key, new VisualsValue(value));

    public void SetVector(string key, Vector3 value) => SetValue(key, new VisualsValue(value));
    
    public void SetColor(string key, Color value) => SetValue(key, new VisualsValue(value));

    public void SetValue(string key, VisualsValue value) => values[key] = value;

    public void Clear() => values.Clear();

    public VisualsValue GetValue(string key, VisualsValue defaultValue) => values.TryGetValue(key, out var value) ? value : defaultValue;
}