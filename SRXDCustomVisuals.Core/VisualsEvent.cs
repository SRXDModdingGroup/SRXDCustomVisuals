namespace SRXDCustomVisuals.Core; 

public class VisualsEvent {
    public VisualsEventType Type { get; }
    
    public int Index { get; }
    
    public double Value { get; }

    public VisualsEvent(VisualsEventType type, int index, double value) {
        Type = type;
        Index = index;
        Value = value;
    }
}