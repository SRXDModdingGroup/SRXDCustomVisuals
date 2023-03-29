using System;

namespace SRXDCustomVisuals.Plugin; 

public class ControlKeyframe : ISequenceElement<ControlKeyframe> {
    public long Time { get; }
    
    public ControlKeyframeType Type { get; }
    
    public int Value { get; }

    public ControlKeyframe(long time, ControlKeyframeType type, int value) {
        Time = time;
        Type = type;
        Value = value;
    }

    public ControlKeyframe(ControlKeyframe other) {
        Time = other.Time;
        Type = other.Type;
        Value = other.Value;
    }

    public ControlKeyframe WithTime(long time) => new(time, Type, Value);

    public ControlKeyframe WithType(ControlKeyframeType type) => new(Time, type, Value);

    public ControlKeyframe WithValue(int value) => new(Time, Type, value);

    public int CompareTo(ControlKeyframe other) => Time.CompareTo(other.Time);

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
            case ControlKeyframeType.EaseOut:
                t = 1f - (1f - t) * (1f - t);
                break;
            case ControlKeyframeType.EaseIn:
                t *= t;
                break;
        }

        return a.Value + t * (b.Value - a.Value);
    }
}