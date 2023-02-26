using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class ElementReference {
    [JsonProperty(PropertyName = "bundle")]
    public string Bundle { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "asset")]
    public string Asset { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "root")]
    public int Root { get; set; }
}