using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class ControlCurve {
    public List<ControlKeyframe> Keyframes { get; } = new();

    public static float Interpolate(ControlKeyframe a, ControlKeyframe b, long time) {
        if (a.Type == ControlKeyframeType.Constant || time < a.Time)
            return a.Value;

        if (time > b.Time)
            return b.Value;

        float t = (float) (time - a.Time) / (b.Time - a.Time);
        
        switch (a.Type) {
            case ControlKeyframeType.Smooth:
                t = t * t * (3f - 2f * t);
                break;
            case ControlKeyframeType.Linear:
                break;
            case ControlKeyframeType.EaseIn:
                t *= t;
                break;
            case ControlKeyframeType.EaseOut:
                t = 1f - (1f - t) * (1f - t);
                break;
        }

        return a.Value + t * (b.Value - a.Value);
    }
}