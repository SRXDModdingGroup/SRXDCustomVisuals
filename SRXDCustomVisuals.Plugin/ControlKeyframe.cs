using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class ControlKeyframe {
    [JsonProperty(propertyName: "time")]
    public long Time { get; set; }
    
    [JsonProperty(propertyName: "value")]
    public byte Value { get; set; }

    public ControlKeyframe(long time, byte value) {
        Time = time;
        Value = value;
    }
}