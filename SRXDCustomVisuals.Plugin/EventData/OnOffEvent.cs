using System;

namespace SRXDCustomVisuals.Plugin; 

public class OnOffEvent : IComparable<OnOffEvent> {
    public long Time { get; }
    
    public OnOffEventType Type { get; }
    
    public int Index { get; }
    
    public int Value { get; }

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