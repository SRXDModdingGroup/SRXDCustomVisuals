namespace SRXDCustomVisuals.Core; 

public class VisualsEvent {
    public VisualsEventType Type { get; }
    
    public byte Channel { get; }
    
    public byte Index { get; }
    
    public byte Value { get; }

    public VisualsEvent(VisualsEventType type, byte channel, byte index = 0, byte value = 255) {
        Type = type;
        Channel = channel;
        Index = index;
        Value = value;
    }
}