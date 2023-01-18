using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class ControlCurve {
    public List<ControlKeyframe> Keyframes { get; }

    public ControlCurve() => Keyframes = new List<ControlKeyframe>();
}