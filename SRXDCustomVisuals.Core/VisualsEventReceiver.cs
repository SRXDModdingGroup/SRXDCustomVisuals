using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsEventReceiver : MonoBehaviour {
    public event Action<VisualsEvent> On;
    
    public event Action<VisualsEvent> Off;
    
    public event Action<VisualsEvent> ControlChange;

    public event Action Reset;

    private void Start() {
        VisualsEventManager.AddReceiver(this);
        Reset?.Invoke();
    }

    private void OnDestroy() => VisualsEventManager.RemoveReceiver(this);

    internal void ReceiveEvent(VisualsEvent visualsEvent) {
        switch (visualsEvent.Type) {
            case VisualsEventType.On:
                On?.Invoke(visualsEvent);
                break;
            case VisualsEventType.Off:
                Off?.Invoke(visualsEvent);
                break;
            case VisualsEventType.ControlChange:
                ControlChange?.Invoke(visualsEvent);
                break;
        }
    }

    internal void DoReset() => Reset?.Invoke();
}