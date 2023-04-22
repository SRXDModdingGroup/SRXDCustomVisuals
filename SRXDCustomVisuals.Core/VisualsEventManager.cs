using System.Collections.Generic;

namespace SRXDCustomVisuals.Core; 

public class VisualsEventManager {
    private List<IVisualsEventHandler> handlers = new();

    public void SendTick(VisualsTick tick) {
        foreach (var receiver in handlers)
            receiver.OnTick(tick);
    }

    public void SendEvent(VisualsEvent visualsEvent) {
        foreach (var receiver in handlers)
            receiver.OnEvent(visualsEvent);
    }

    public void ResetAll() {
        foreach (var receiver in handlers)
            receiver.OnReset();
    }

    public void AddHandler(IVisualsEventHandler handler) => handlers.Add(handler);

    public void RemoveHandler(IVisualsEventHandler handler) => handlers.Remove(handler);

    public void ClearHandlers() => handlers.Clear();
}