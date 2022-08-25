using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsEventBuilder {
    private List<float> data = new();

    public unsafe void AddInt(int val) => data.Add(*(float*) &val);

    public void AddFloat(float val) => data.Add(val);

    public void AddVector2(Vector2 val) {
        AddFloat(val.x);
        AddFloat(val.y);
    }
    
    public void AddVector3(Vector3 val) {
        AddFloat(val.x);
        AddFloat(val.y);
        AddFloat(val.z);
    }
    
    public void AddColor3(Color val) {
        AddFloat(val.r);
        AddFloat(val.g);
        AddFloat(val.b);
    }
    
    public void AddColor4(Color val) {
        AddFloat(val.r);
        AddFloat(val.g);
        AddFloat(val.b);
        AddFloat(val.a);
    }

    internal VisualsEvent Build() => new(data);
}