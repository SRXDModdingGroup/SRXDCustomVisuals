using System;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class BackgroundDefinition {
    [JsonProperty("disableBaseBackground")]
    public bool DisableBaseBackground { get; set; } = true;
    
    [JsonProperty("useAudioSpectrum")]
    public bool UseAudioSpectrum { get; set; }
    
    [JsonProperty("useAudioWaveform")]
    public bool UseAudioWaveform { get; set; }

    [JsonProperty("useDepthTexture")]
    public bool UseDepthTexture { get; set; }

    [JsonProperty("farClip")]
    public float FarClip { get; set; } = 100f;

    [JsonProperty("eventLabels")]
    public string[] EventLabels { get; set; } = Array.Empty<string>();
    
    [JsonProperty("curveLabels")]
    public string[] CurveLabels { get; set; } = Array.Empty<string>();

    [JsonProperty("assetBundles")]
    public string[] AssetBundles { get; set; } = Array.Empty<string>();

    [JsonProperty("elements")]
    public ElementReference[] Elements { get; set; } = Array.Empty<ElementReference>();

    [JsonProperty("dependencies")]
    public string[] Dependencies { get; set; } = Array.Empty<string>();
}