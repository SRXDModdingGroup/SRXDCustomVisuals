using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsEventManager : MonoBehaviour {
    public static VisualsEventManager Instance { get; private set; }

    private List<VisualsEventReceiver> receivers;

    private void Awake() {
        Instance = this;
        receivers = new List<VisualsEventReceiver>();
    }
    
    public void SendEvent(VisualsEvent visualsEvent) {
        foreach (var receiver in receivers) {
            if (receiver != null)
                receiver.ReceiveEvent(visualsEvent);
        }
    }

    public void ResetAll() {
        foreach (var receiver in receivers) {
            if (receiver != null)
                receiver.DoReset();
        }
    }

    internal void AddReceiver(VisualsEventReceiver receiver) => receivers.Add(receiver);

    internal void RemoveReceiver(VisualsEventReceiver receiver) => receivers.Remove(receiver);
}