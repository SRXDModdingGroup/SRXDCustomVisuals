namespace SRXDCustomVisuals.Core; 

public class VisualsEvent {
    public VisualsEventType Type { get; }
    
    public int Index { get; }
    
    public float Value { get; }

    public VisualsEvent(VisualsEventType type, int index, float value) {
        Type = type;
        Index = index;
        Value = value;
    }
}