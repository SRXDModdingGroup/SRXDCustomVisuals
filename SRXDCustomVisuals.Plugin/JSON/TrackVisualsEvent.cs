using System;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEvent : IComparable<TrackVisualsEvent> {
    [JsonProperty("time")]
    public long Time { get; }
    
    [JsonProperty("type")]
    public TrackVisualsEventType Type { get; }
    
    [JsonProperty("keyframeType")]
    public ControlKeyframeType KeyframeType { get; }
    
    [JsonProperty("index")]
    public int Index { get; }
    
    [JsonProperty("value")]
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