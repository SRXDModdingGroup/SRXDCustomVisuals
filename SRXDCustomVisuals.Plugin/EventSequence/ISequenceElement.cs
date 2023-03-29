using System;

namespace SRXDCustomVisuals.Plugin; 

public interface ISequenceElement<T> : IComparable<T> {
    long Time { get; }
    
    int Value { get; }

    public T WithTime(long time);

    public T WithValue(int value);
}