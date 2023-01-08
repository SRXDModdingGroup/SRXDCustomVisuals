using System.Collections.Generic;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class CustomVisualsInfo {
    [JsonProperty(propertyName: "background")]
    public string Background { get; set; }
    
    [JsonProperty(propertyName: "events")]
    public List<TrackVisualsEvent> Events { get; set; }
}