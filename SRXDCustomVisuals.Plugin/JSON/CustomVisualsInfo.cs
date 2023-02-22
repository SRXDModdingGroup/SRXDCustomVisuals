using System.Collections.Generic;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class CustomVisualsInfo {
    [JsonProperty(PropertyName = "background")]
    public string Background { get; set; }
    
    [JsonProperty(PropertyName = "palette")]
    public List<PaletteColor> Palette { get; set; }

    [JsonProperty(PropertyName = "events")]
    public List<TrackVisualsEvent> Events { get; set; }

    public bool IsEmpty() => string.IsNullOrWhiteSpace(Background) && (Events == null || Events.Count == 0);

    public CustomVisualsInfo() {
        Background = "";
        Events = new List<TrackVisualsEvent>();
    }
    
    public CustomVisualsInfo(string background, List<PaletteColor> palette, List<TrackVisualsEvent> events) {
        Background = background;
        Palette = palette;
        Events = events;
    }
}