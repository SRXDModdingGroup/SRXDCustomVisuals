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

    public OnOffEvent(OnOffEvent other) {
        Time = other.Time;
        Type = other.Type;
        Index = other.Index;
        Value = other.Value;
    }

    public OnOffEvent WithTime(long time) => new(time, Type, Index, Value);
    
    public OnOffEvent WithType(OnOffEventType type) => new(Time, type, Index, Value);

    public OnOffEvent WithIndex(int index) => new(Time, Type, index, Value);
    
    public OnOffEvent WithValue(int value) => new(Time, Type, Index, value);

    public int CompareTo(OnOffEvent other) {
        int timeComparison = Time.CompareTo(other.Time);

        if (timeComparison != 0)
            return timeComparison;

        return Index.CompareTo(other.Index);
    }
}