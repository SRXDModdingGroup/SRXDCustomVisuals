using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsProperty : IVisualsProperty {
    private Vector4 currentValue;

    public event Action Changed;
    
    public void SetBool(bool value) => SetValue(new Vector4(value ? 1f : 0f, 0f, 0f, 0f));

    public void SetInt(int value) => SetValue(new Vector4(value, 0f, 0f, 0f));

    public void SetFloat(float value) => SetValue(new Vector4(value, 0f, 0f, 0f));

    public void SetVector(Vector3 value) => SetValue(value);

    public void SetColor(Color value) => SetValue(value);

    public bool GetBool() => currentValue.x > 0f;

    public int GetInt() => Mathf.RoundToInt(currentValue.x);

    public float GetFloat() => currentValue.x;

    public Vector3 GetVector() => currentValue;

    public Color GetColor() => currentValue;

    private void SetValue(Vector4 value) {
        if (value == currentValue)
            return;

        currentValue = value;
        Changed?.Invoke();
    }
}