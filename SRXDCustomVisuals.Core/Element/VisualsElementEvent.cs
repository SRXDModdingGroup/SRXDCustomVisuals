using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsElementEvent {
    public string name;

    public VisualsEventMapping[] mappings;
}