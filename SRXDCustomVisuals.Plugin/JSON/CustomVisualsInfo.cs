using System.Collections.Generic;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class CustomVisualsInfo {
    [JsonProperty(propertyName: "background")]
    public string Background { get; set; }

    [JsonProperty(propertyName: "events")]
    public List<TrackVisualsEvent> Events { get; set; }

    public bool IsEmpty() => string.IsNullOrWhiteSpace(Background) && (Events == null || Events.Count == 0);

    public CustomVisualsInfo() {
        Background = "";
        Events = new List<TrackVisualsEvent>();
    }
    
    public CustomVisualsInfo(string background, List<TrackVisualsEvent> events) {
        Background = background;
        Events = events;
    }
}