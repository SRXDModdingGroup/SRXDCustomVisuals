using UnityEngine;

namespace SRXDCustomVisuals.Core.Value; 

public readonly struct VisualsValue {
    private readonly Vector4 value;

    public bool Bool => value.x > 0f;

    public int Int => Mathf.RoundToInt(value.x);

    public float Float => value.x;

    public Vector3 Vector => value;

    public Color Color => value;

    public VisualsValue(bool value) : this(value ? 1f : 0f) { }

    public VisualsValue(int value) : this((float) value) { }

    public VisualsValue(float value) => this.value = new Vector4(value, value, value, 1f);

    public VisualsValue(Vector3 value) => this.value = new Vector4(value.x, value.y, value.z, 1f);

    public VisualsValue(Color value) => this.value = value;

    public static bool operator ==(VisualsValue a, VisualsValue b) => a.value == b.value;

    public static bool operator !=(VisualsValue a, VisualsValue b) => a.value != b.value;

    public override bool Equals(object obj) => obj is VisualsValue other && this == other;

    public override int GetHashCode() => value.GetHashCode();
}