using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsEventBinding {
    public string eventName;

    public VisualsEventMapping[] mappings;
}