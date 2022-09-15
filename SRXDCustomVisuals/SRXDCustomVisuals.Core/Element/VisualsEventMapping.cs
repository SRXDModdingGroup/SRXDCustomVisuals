using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsEventMapping {
    public string name;
    
    public VisualsController target;

    public VisualsParamMapping[] parameters;
}