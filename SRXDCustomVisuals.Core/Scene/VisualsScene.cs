using System.Collections.Generic;

namespace SRXDCustomVisuals.Core; 

public class VisualsScene {
    internal List<VisualsElement> Elements { get; }
    
    public VisualsScene(IEnumerable<VisualsElement> elements) => Elements = new List<VisualsElement>(elements);
}