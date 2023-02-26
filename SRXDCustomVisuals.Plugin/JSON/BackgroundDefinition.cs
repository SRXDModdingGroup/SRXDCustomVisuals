using System;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class BackgroundDefinition {
    [JsonProperty(PropertyName = "disableBaseBackground")]
    public bool DisableBaseBackground { get; set; }
    
    [JsonProperty(PropertyName = "useAudioSpectrum")]
    public bool UseAudioSpectrum { get; set; }

    [JsonProperty(PropertyName = "assetBundles")]
    public string[] AssetBundles { get; set; } = Array.Empty<string>();

    [JsonProperty(PropertyName = "assemblies")]
    public string[] Assemblies { get; set; } = Array.Empty<string>();

    [JsonProperty(PropertyName = "elements")]
    public ElementReference[] Elements { get; set; } = Array.Empty<ElementReference>();
}