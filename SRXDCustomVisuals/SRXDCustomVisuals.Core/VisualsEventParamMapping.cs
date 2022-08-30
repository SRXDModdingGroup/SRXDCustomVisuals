using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsEventParamMapping {
    public string from;

    public string to;
    
    public VisualsEventParamType type;
}