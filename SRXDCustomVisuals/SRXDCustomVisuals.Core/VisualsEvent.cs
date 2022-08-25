using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsEvent {
    internal static VisualsEvent Empty { get; } = new(new List<float>());
    
    private List<float> data;
    private int index;
    
    internal VisualsEvent(List<float> data) {
        this.data = data;
    }

    public unsafe int GetInt() {
        if (index >= data.Count)
            return 0;
        
        float val = data[index];
        
        index++;

        return *(int*) &val;
    }

    public float GetFloat() {
        if (index >= data.Count)
            return 0f;
        
        float val = data[index];

        index++;

        return val;
    }

    public Vector2 GetVector2() => new(GetFloat(), GetFloat());

    public Vector3 GetVector3() => new(GetFloat(), GetFloat(), GetFloat());

    public Color GetColor3() => new(GetFloat(), GetFloat(), GetFloat());

    public Color GetColor4() => new(GetFloat(), GetFloat(), GetFloat(), GetFloat());

    internal void Reset() => index = 0;
}