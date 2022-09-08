using System;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class CustomVisualsInfo {
    [JsonProperty(propertyName: "hasCustomVisuals")]
    public bool HasCustomVisuals { get; set; }

    [JsonProperty(propertyName: "disableBaseBackground")]
    public bool DisableBaseBackground { get; set; }

    [JsonProperty(propertyName: "assetBundles")]
    public string[] AssetBundles { get; set; } = Array.Empty<string>();

    [JsonProperty(propertyName: "modules")]
    public VisualsModuleReference[] Modules { get; set; } = Array.Empty<VisualsModuleReference>();
}