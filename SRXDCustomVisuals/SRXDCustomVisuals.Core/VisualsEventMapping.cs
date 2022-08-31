using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsEventMapping {
    public VisualsEventTarget target;
    
    public string action;

    public VisualsEventParamMapping[] parameters;
}