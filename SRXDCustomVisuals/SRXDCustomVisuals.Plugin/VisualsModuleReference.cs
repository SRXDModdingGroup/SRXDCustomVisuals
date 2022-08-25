using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsModuleReference {
    [JsonProperty(propertyName: "assetBundleName")]
    public string AssetBundleName { get; set; }
    
    [JsonProperty(propertyName: "assetName")]
    public string AssetName { get; set; }
}