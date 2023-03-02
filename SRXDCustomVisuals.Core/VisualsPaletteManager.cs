using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public static class VisualsPaletteManager {
    internal static IReadOnlyList<Color> Palette { get; private set; } = Array.Empty<Color>();

    private static List<VisualsPaletteReceiver> receivers = new();

    public static void SetPalette(IReadOnlyList<Color> palette) {
        Palette = palette;

        foreach (var receiver in receivers)
            receiver.DoPaletteChanged(palette);
    }

    internal static void AddReceiver(VisualsPaletteReceiver receiver) => receivers.Add(receiver);

    internal static void RemoveReceiver(VisualsPaletteReceiver receiver) => receivers.Remove(receiver);
}