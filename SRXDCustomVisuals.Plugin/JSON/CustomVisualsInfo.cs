using System.Collections.Generic;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class CustomVisualsInfo {
    [JsonProperty("background")]
    public string Background { get; set; } = string.Empty;

    [JsonProperty("palette")]
    public List<PaletteColor> Palette { get; set; } = new();

    [JsonProperty("customData")]
    public Dictionary<string, string> CustomData { get; set; } = new();

    [JsonProperty("events")]
    public List<TrackVisualsEvent> Events { get; set; } = new();

    public bool IsEmpty() => string.IsNullOrWhiteSpace(Background) && (Events == null || Events.Count == 0);

    public CustomVisualsInfo() { }
    
    public CustomVisualsInfo(string background, List<PaletteColor> palette, Dictionary<string, string> customData, List<TrackVisualsEvent> events) {
        Background = background;
        Palette = palette;
        CustomData = customData;
        Events = events;
    }
}