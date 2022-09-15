using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsPropertyMapping {
    public string name;
    
    public VisualsController target;
    
    public VisualsParamType type;
}