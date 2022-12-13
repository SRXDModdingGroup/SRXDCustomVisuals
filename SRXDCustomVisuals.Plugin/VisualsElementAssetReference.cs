using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsElementAssetReference {
    [JsonProperty(propertyName: "bundle")]
    public string Bundle { get; set; } = string.Empty;
    
    [JsonProperty(propertyName: "asset")]
    public string Asset { get; set; } = string.Empty;
    
    [JsonProperty(propertyName: "root")]
    public int Root { get; set; }
}