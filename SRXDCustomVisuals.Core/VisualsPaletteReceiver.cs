using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsPaletteReceiver : MonoBehaviour {
    public IReadOnlyList<Color> Palette => VisualsPaletteManager.Palette;

    public event Action<IReadOnlyList<Color>> PaletteChanged;

    private void Start() {
        VisualsPaletteManager.AddReceiver(this);
        PaletteChanged?.Invoke(VisualsPaletteManager.Palette);
    }

    private void OnDestroy() => VisualsPaletteManager.RemoveReceiver(this);
    
    internal void DoPaletteChanged(IReadOnlyList<Color> palette) => PaletteChanged?.Invoke(palette);
}