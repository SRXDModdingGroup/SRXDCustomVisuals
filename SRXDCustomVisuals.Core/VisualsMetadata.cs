using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsMetadata {
    public static VisualsMetadata Empty { get; } = new(Array.Empty<Color>(), new Dictionary<string, string>());
    
    private List<Color> colors;
    private Dictionary<string, string> customData;

    public VisualsMetadata(IEnumerable<Color> colors, IDictionary<string, string> customData) {
        this.colors = new List<Color>(colors);
        this.customData = new Dictionary<string, string>(customData);
    }

    public Color GetColor(int index) => GetColor(index, Color.white);

    public Color GetColor(int index, Color defaultColor) {
        if (index < 0)
            throw new ArgumentOutOfRangeException();

        return index < colors.Count ? colors[index] : defaultColor;
    }

    public string GetCustomValue(string key) => GetCustomValue(key, string.Empty);

    public string GetCustomValue(string key, string defaultValue) {
        if (customData.TryGetValue(key, out string value))
            return value;

        return defaultValue;
    }
}