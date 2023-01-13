namespace SRXDCustomVisuals.Core; 

public class VisualsEvent {
    public VisualsEventType Type { get; }
    
    public byte Channel { get; }
    
    public byte Index { get; }
    
    public uint Value { get; }

    public VisualsEvent(VisualsEventType type, byte channel, byte index, uint value) {
        Type = type;
        Channel = channel;
        Index = index;
        Value = value;
    }
}