namespace SRXDCustomVisuals.Core; 

internal class EmptyVisualsEvent : IVisualsEvent {
    public static EmptyVisualsEvent Instance { get; } = new();
    
    public void Invoke(IVisualsParams parameters) { }
}