namespace SRXDCustomVisuals.Core; 

internal class EmptyVisualsProperty : IVisualsProperty {
    public static EmptyVisualsProperty Instance { get; } = new();
    
    public void SetValue(VisualsValue value) { }
}