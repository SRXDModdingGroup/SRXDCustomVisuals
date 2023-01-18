using System;

namespace SRXDCustomVisuals.Plugin; 

public class OnOffEvent : IComparable<OnOffEvent> {
    public long Time { get; set; }
    
    public OnOffEventType Type { get; set; }
    
    public byte Index { get; set; }
    
    public byte Value { get; set; }

    public OnOffEvent(long time, OnOffEventType type, byte index, byte value) {
        Time = time;
        Type = type;
        Index = index;
        Value = value;
    }

    public int CompareTo(OnOffEvent other) => Time.CompareTo(other.Time);
}