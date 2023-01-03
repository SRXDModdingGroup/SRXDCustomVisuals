using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsEventReceiver : MonoBehaviour {
    [SerializeField] private int channel;

    public int Channel => channel;

    public event Action<VisualsEvent> OnNoteOn;
    
    public event Action<VisualsEvent> OnNoteOff;
    
    public event Action<VisualsEvent> OnControlChange;

    private void Awake() {
        if (channel is < 0 or >= 256 )
            return;

        VisualsEventManager.Instance.AddReceiver(this);
    }

    private void OnDestroy() {
        if (channel is < 0 or >= 256 )
            return;
        
        VisualsEventManager.Instance.RemoveReceiver(this);
    }

    internal void ReceiveEvent(VisualsEvent visualsEvent) {
        switch (visualsEvent.Type) {
            case VisualsEventType.NoteOn:
                OnNoteOn?.Invoke(visualsEvent);
                break;
            case VisualsEventType.NoteOff:
                OnNoteOff?.Invoke(visualsEvent);
                break;
            case VisualsEventType.ControlChange:
                OnControlChange?.Invoke(visualsEvent);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}