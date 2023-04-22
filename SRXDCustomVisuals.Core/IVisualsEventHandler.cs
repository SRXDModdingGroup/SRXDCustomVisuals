namespace SRXDCustomVisuals.Core; 

public interface IVisualsEventHandler {
    void OnTick(VisualsTick tick);

    void OnEvent(VisualsEvent visualsEvent);

    void OnReset();
}