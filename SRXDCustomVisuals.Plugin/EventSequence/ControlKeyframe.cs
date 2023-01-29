using System;

namespace SRXDCustomVisuals.Plugin; 

public class ControlKeyframe : IComparable<ControlKeyframe> {
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
}