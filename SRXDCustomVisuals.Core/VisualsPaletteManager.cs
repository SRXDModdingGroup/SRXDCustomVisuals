using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsPaletteManager : MonoBehaviour {
    public static VisualsPaletteManager Instance { get; private set; }
    
    internal IReadOnlyList<Color> Palette { get; private set; }

    private List<VisualsPaletteReceiver> receivers;

    private void Awake() {
        Instance = this;
        receivers = new List<VisualsPaletteReceiver>();
    }

    public void SetPalette(IReadOnlyList<Color> palette) {
        Palette = palette;

        foreach (var receiver in receivers)
            receiver.DoPaletteChanged(palette);
    }

    internal void AddReceiver(VisualsPaletteReceiver receiver) => receivers.Add(receiver);

    internal void RemoveReceiver(VisualsPaletteReceiver receiver) => receivers.Remove(receiver);
}