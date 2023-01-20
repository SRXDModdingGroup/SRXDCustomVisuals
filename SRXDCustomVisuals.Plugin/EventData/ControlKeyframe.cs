using System;

namespace SRXDCustomVisuals.Plugin; 

public class ControlKeyframe : IComparable<ControlKeyframe> {
    public long Time { get; set; }
    
    public ControlKeyframeType Type { get; set; }
    
    public int Value { get; set; }

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

    public int CompareTo(ControlKeyframe other) => Time.CompareTo(other.Time);
}