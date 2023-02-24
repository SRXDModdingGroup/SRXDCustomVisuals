using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsPaletteManager : MonoBehaviour {
    public static VisualsPaletteManager Instance { get; private set; }
    
    private IReadOnlyList<Color> palette;
    private List<VisualsPaletteReceiver> receivers;

    private void Awake() {
        Instance = this;
        receivers = new List<VisualsPaletteReceiver>();
    }

    public void SetPalette(IReadOnlyList<Color> palette) {
        this.palette = palette;

        foreach (var receiver in receivers)
            receiver.SetPalette(palette);
    }

    internal void AddReceiver(VisualsPaletteReceiver receiver) {
        receivers.Add(receiver);
        receiver.SetPalette(palette);
    }

    internal void RemoveReceiver(VisualsPaletteReceiver receiver) => receivers.Remove(receiver);
}