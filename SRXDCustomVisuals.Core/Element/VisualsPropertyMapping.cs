using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsPropertyMapping {
    public string name;
    
    public VisualsController target;
    
    public VisualsParamType type;

    public Vector4 scale = Vector4.one;

    public Vector4 bias;
}