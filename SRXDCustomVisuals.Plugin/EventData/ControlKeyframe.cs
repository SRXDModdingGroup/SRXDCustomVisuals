using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class ControlKeyframe {
    public long Time { get; set; }
    
    public ControlKeyframeType Type { get; }
    
    public byte Value { get; set; }

    public ControlKeyframe(long time, ControlKeyframeType type, byte value) {
        Time = time;
        Value = value;
        Type = type;
    }
}