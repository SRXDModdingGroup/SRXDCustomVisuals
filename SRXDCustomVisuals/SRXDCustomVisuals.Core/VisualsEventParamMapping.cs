using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsEventParamMapping {
    public string name;
    
    public VisualsEventParamType type;
    
    public string value;
}