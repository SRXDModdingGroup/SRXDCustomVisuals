using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsPaletteReceiver : MonoBehaviour {
    public IReadOnlyList<Color> Palette => VisualsPaletteManager.Instance.Palette;

    public event Action<IReadOnlyList<Color>> PaletteChanged;

    private void Start() {
        VisualsPaletteManager.Instance.AddReceiver(this);
        PaletteChanged?.Invoke(VisualsPaletteManager.Instance.Palette);
    }

    private void OnDestroy() => VisualsPaletteManager.Instance.RemoveReceiver(this);
    
    internal void DoPaletteChanged(IReadOnlyList<Color> palette) => PaletteChanged?.Invoke(palette);
}