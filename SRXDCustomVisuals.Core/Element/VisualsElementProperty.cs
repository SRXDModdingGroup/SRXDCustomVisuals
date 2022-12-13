using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsElementProperty {
    public string name;

    public VisualsPropertyMapping[] mappings;
}