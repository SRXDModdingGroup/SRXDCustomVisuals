using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualElementEvent {
    public string name;

    public VisualsEventMapping[] mappings;
}