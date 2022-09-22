using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsParamMapping {
    public string name;
    
    public VisualsParamType type;
    
    public string parameter;

    public Vector4 scale = Vector4.one;

    public Vector4 bias;
}