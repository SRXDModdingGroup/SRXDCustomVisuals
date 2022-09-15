using System.Collections.Generic;

namespace SRXDCustomVisuals.Core; 

public class VisualsScene {
    internal List<VisualElement> Elements { get; }
    
    public VisualsScene(IEnumerable<VisualElement> elements) => Elements = new List<VisualElement>(elements);
}