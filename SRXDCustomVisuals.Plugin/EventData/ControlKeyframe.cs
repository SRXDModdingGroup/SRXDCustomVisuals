namespace SRXDCustomVisuals.Plugin; 

public class ControlKeyframe {
    public long Time { get; }
    
    public ControlKeyframeType Type { get; }
    
    public int Value { get; }

    public ControlKeyframe(long time, ControlKeyframeType type, int value) {
        Time = time;
        Value = value;
        Type = type;
    }
}