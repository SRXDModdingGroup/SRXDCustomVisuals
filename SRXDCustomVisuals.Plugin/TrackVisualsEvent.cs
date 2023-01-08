using Newtonsoft.Json;
using SRXDCustomVisuals.Core;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEvent {
    [JsonProperty(propertyName: "time")]
    public long Time { get; }
    
    [JsonProperty(propertyName: "type")]
    public VisualsEventType Type { get; }
    
    [JsonProperty(propertyName: "channel")]
    public int Channel { get; }
    
    [JsonProperty(propertyName: "index")]
    public int Index { get; }
    
    [JsonProperty(propertyName: "value")]
    public int Value { get; }

    public TrackVisualsEvent(long time, VisualsEventType type, int channel, int index, int value) {
        Time = time;
        Type = type;
        Channel = channel;
        Index = index;
        Value = value;
    }
}