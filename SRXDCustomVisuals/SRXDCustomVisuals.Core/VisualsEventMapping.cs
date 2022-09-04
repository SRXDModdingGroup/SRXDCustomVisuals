using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsEventMapping {
    public VisualsController target;
    
    public string action;

    public VisualsEventParamMapping[] parameters;
}