using System;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class BackgroundDefinition {
    [JsonProperty(propertyName: "disableBaseBackground")]
    public bool DisableBaseBackground { get; set; }

    [JsonProperty(propertyName: "assetBundles")]
    public string[] AssetBundles { get; set; } = Array.Empty<string>();

    [JsonProperty(propertyName: "assemblies")]
    public string[] Assemblies { get; set; } = Array.Empty<string>();

    [JsonProperty(propertyName: "elements")]
    public VisualsElementAssetReference[] Elements { get; set; } = Array.Empty<VisualsElementAssetReference>();
}