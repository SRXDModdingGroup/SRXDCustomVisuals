using System;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEvent : IComparable<TrackVisualsEvent> {
    [JsonProperty(propertyName: "time")]
    public long Time { get; }
    
    [JsonProperty(propertyName: "type")]
    public TrackVisualsEventType Type { get; }
    
    [JsonProperty(propertyName: "keyframeType")]
    public ControlKeyframeType KeyframeType { get; }
    
    [JsonProperty(propertyName: "index")]
    public int Index { get; }
    
    [JsonProperty(propertyName: "value")]
    public int Value { get; }

    public TrackVisualsEvent(long time, TrackVisualsEventType type, ControlKeyframeType keyframeType, int index, int value) {
        Time = time;
        Type = type;
        KeyframeType = keyframeType;
        Index = index;
        Value = value;
    }

    public int CompareTo(TrackVisualsEvent other) {
        int timeComparison = Time.CompareTo(other.Time);

        if (timeComparison != 0)
            return timeComparison;

        return Index.CompareTo(other.Index);
    }
}