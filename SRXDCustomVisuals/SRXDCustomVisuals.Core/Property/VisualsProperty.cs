using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsProperty : IVisualsProperty {
    private Vector4 value = Vector4.positiveInfinity;
    private Action<VisualsProperty> changed;

    public VisualsProperty(Action<VisualsProperty> changed = null) => this.changed = changed;

    public void SetBool(bool value) => SetFloat(value ? 1f : 0f);

    public void SetInt(int value) => SetFloat(value);

    public void SetFloat(float value) => SetValue(new Vector4(value, value, value, 1f));

    public void SetVector(Vector3 value) => SetValue(new Vector4(value.x, value.y, value.z, 1f));

    public void SetColor(Color value) => SetValue(value);

    public bool GetBool() => value.x > 0f;

    public int GetInt() => Mathf.RoundToInt(value.x);

    public float GetFloat() => value.x;

    public Vector3 GetVector() => value;

    public Color GetColor() => value;

    private void SetValue(Vector4 value) {
        if (value == this.value)
            return;

        this.value = value;
        changed?.Invoke(this);
    }
}