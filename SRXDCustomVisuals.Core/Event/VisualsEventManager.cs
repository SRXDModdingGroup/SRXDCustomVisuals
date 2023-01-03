using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsEventManager : MonoBehaviour {
    public static VisualsEventManager Instance { get; private set; }

    private List<VisualsEventReceiver>[] receivers;

    private void Awake() {
        Instance = this;
        receivers = new List<VisualsEventReceiver>[256];

        for (int i = 0; i < 256; i++)
            receivers[i] = new List<VisualsEventReceiver>();
    }
    
    public void SendEvent(VisualsEvent visualsEvent) {
        foreach (var receiver in receivers[visualsEvent.Channel])
            receiver.ReceiveEvent(visualsEvent);
    }

    internal void AddReceiver(VisualsEventReceiver receiver) => receivers[receiver.Channel].Add(receiver);

    internal void RemoveReceiver(VisualsEventReceiver receiver) => receivers[receiver.Channel].Remove(receiver);
}