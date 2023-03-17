using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class PaletteColor {
    [JsonProperty("red")]
    public int Red { get; set; }
    
    [JsonProperty("green")]
    public int Green { get; set; }
    
    [JsonProperty("blue")]
    public int Blue { get; set; }

    public PaletteColor() { }

    public PaletteColor(int red, int green, int blue) {
        Red = red;
        Green = green;
        Blue = blue;
    }
}