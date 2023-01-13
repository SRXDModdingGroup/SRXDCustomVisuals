using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class ControlCurve {
    public int Index { get; set; }
    
    public List<ControlKeyframe> Keyframes { get; }

    public ControlCurve(int index) {
        Index = index;
        Keyframes = new List<ControlKeyframe>();
    }
}