using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsPropertyMapping {
    public VisualsController target;
    
    public string name;
    
    public VisualsParamType type;

    public Vector4 scale = Vector4.one;

    public Vector4 bias;
}