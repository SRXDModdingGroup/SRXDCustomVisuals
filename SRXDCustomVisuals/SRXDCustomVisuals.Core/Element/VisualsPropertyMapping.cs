using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsPropertyMapping {
    public VisualsController target;
    
    public string name;
    
    public VisualsParamType type;
}