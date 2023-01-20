using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class ControlCurve {
    public List<ControlKeyframe> Keyframes { get; }

    public ControlCurve() => Keyframes = new List<ControlKeyframe>();

    public static double Interpolate(ControlKeyframe a, ControlKeyframe b, long time) {
        if (a.Type == ControlKeyframeType.Constant)
            return a.Value;

        double t = (double) (time - a.Time) / (b.Time - a.Time);
        
        switch (a.Type) {
            case ControlKeyframeType.Smooth:
                t = t * t * (3d - 2d * t);
                break;
            case ControlKeyframeType.Linear:
                break;
            case ControlKeyframeType.EaseIn:
                t *= t;
                break;
            case ControlKeyframeType.EaseOut:
                t = 1d - (1d - t) * (1d - t);
                break;
            case ControlKeyframeType.Constant:
            default:
                throw new ArgumentOutOfRangeException();
        }

        return a.Value + t * (b.Value - a.Value);
    }
}