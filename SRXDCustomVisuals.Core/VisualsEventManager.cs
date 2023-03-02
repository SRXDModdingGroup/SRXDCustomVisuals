using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public static class VisualsEventManager {
    private static List<VisualsEventReceiver> receivers = new();

    public static void SendEvent(VisualsEvent visualsEvent) {
        foreach (var receiver in receivers) {
            if (receiver != null)
                receiver.ReceiveEvent(visualsEvent);
        }
    }

    public static void ResetAll() {
        foreach (var receiver in receivers) {
            if (receiver != null)
                receiver.DoReset();
        }
    }

    internal static void AddReceiver(VisualsEventReceiver receiver) => receivers.Add(receiver);

    internal static void RemoveReceiver(VisualsEventReceiver receiver) => receivers.Remove(receiver);
}