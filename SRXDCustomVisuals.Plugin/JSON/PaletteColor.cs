using Newtonsoft.Json;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin; 

public class PaletteColor {
    [JsonProperty("red")]
    public int Red { get; set; }
    
    [JsonProperty("green")]
    public int Green { get; set; }
    
    [JsonProperty("blue")]
    public int Blue { get; set; }

    public PaletteColor() { }

    public PaletteColor(Color32 color) {
        Red = color.r;
        Green = color.g;
        Blue = color.b;
    }

    public PaletteColor(int red, int green, int blue) {
        Red = red;
        Green = green;
        Blue = blue;
    }

    public Color ToColor() => new(Red / 255f, Green / 255f, Blue / 255f);
}