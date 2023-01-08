using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class NoteOnOffEvent {
    [JsonProperty(propertyName: "time")]
    public long Time { get; set; }
    
    [JsonProperty(propertyName: "on")]
    public bool On { get; set; }
    
    [JsonProperty(propertyName: "index")]
    public byte Index { get; set; }
    
    [JsonProperty(propertyName: "value")]
    public byte Value { get; set; }

    public NoteOnOffEvent(long time, bool on, byte index, byte value) {
        Time = time;
        On = on;
        Index = index;
        Value = value;
    }
}