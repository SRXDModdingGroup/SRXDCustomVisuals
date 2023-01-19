using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsEventReceiver : MonoBehaviour {
    public event Action<VisualsEvent> On;
    
    public event Action<VisualsEvent> Off;
    
    public event Action<VisualsEvent> ControlChange;

    public event Action OnReset;

    private void Awake() => VisualsEventManager.Instance.AddReceiver(this);

    private void OnDestroy() => VisualsEventManager.Instance.RemoveReceiver(this);

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
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal void Reset() => OnReset?.Invoke();
}