using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualElementProperty {
    public string name;

    public VisualsPropertyMapping[] mappings;
}