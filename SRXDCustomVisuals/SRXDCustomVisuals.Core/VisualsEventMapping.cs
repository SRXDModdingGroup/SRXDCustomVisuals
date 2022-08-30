using System;

namespace SRXDCustomVisuals.Core; 

[Serializable]
public class VisualsEventMapping {
    public VisualsEvent target;

    public VisualsEventParamMapping[] parameterMappings;
}