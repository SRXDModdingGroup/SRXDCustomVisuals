using System;

namespace SRXDCustomVisuals.Plugin; 

public class OnOffEvent : ISequenceElement<OnOffEvent> {
    public long Time { get; }
    
    public OnOffEventType Type { get; }
    
    public int Value { get; }

    public OnOffEvent(long time, OnOffEventType type, int value) {
        Time = time;
        Type = type;
        Value = value;
    }

    public OnOffEvent(OnOffEvent other) {
        Time = other.Time;
        Type = other.Type;
        Value = other.Value;
    }

    public OnOffEvent WithTime(long time) => new(time, Type, Value);
    
    public OnOffEvent WithType(OnOffEventType type) => new(Time, type, Value);
    
    public OnOffEvent WithValue(int value) => new(Time, Type, value);

    public int CompareTo(OnOffEvent other) => Time.CompareTo(other.Time);
}