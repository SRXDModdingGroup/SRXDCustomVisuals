using System;

namespace SRXDCustomVisuals.Plugin; 

public class OnOffEvent : IComparable<OnOffEvent> {
    public long Time { get; set; }
    
    public OnOffEventType Type { get; set; }
    
    public int Index { get; set; }
    
    public int Value { get; set; }

    public OnOffEvent(long time, OnOffEventType type, int index, int value) {
        Time = time;
        Type = type;
        Index = index;
        Value = value;
    }

    public int CompareTo(OnOffEvent other) {
        int timeComparison = Time.CompareTo(other.Time);

        if (timeComparison != 0)
            return timeComparison;

        return Index.CompareTo(other.Index);
    }
}