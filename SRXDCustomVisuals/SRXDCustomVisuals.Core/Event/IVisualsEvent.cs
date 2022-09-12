namespace SRXDCustomVisuals.Core; 

public interface IVisualsEvent {
    void Invoke(IVisualsParams parameters);
}

public static class VisualsEventExtensions {
    public static void Invoke(this IVisualsEvent visualsEvent) => visualsEvent.Invoke(VisualsParams.Empty);
}