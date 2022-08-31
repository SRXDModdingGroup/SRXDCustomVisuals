using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsEvent {
    public string name;

    public VisualsEventMapping[] mappings;
}