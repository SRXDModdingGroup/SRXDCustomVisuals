using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsPaletteReceiver : MonoBehaviour {
    public IReadOnlyList<Color> Palette { get; private set; }

    public event Action<IReadOnlyList<Color>> PaletteChanged;

    private void Start() => VisualsPaletteManager.Instance.AddReceiver(this);

    private void OnDestroy() => VisualsPaletteManager.Instance.RemoveReceiver(this);
    
    internal void SetPalette(IReadOnlyList<Color> palette) {
        Palette = palette;
        PaletteChanged?.Invoke(palette);
    }
}