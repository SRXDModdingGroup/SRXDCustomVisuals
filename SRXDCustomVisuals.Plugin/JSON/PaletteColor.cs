using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class PaletteColor {
    [JsonProperty(PropertyName = "red")]
    public int Red { get; set; }
    
    [JsonProperty(PropertyName = "green")]
    public int Green { get; set; }
    
    [JsonProperty(PropertyName = "blue")]
    public int Blue { get; set; }

    public PaletteColor() { }

    public PaletteColor(int red, int green, int blue) {
        Red = red;
        Green = green;
        Blue = blue;
    }
}