using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsModuleReference {
    [JsonProperty(propertyName: "bundle")]
    public string Bundle { get; set; } = string.Empty;
    
    [JsonProperty(propertyName: "asset")]
    public string Asset { get; set; } = string.Empty;
}