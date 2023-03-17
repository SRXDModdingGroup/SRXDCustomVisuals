using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class ElementReference {
    [JsonProperty("bundle")]
    public string Bundle { get; set; } = string.Empty;
    
    [JsonProperty("asset")]
    public string Asset { get; set; } = string.Empty;
    
    [JsonProperty("root")]
    public int Root { get; set; }
}