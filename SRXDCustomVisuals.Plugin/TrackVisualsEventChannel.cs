using System.Collections.Generic;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventChannel {
    [JsonProperty(propertyName: "index")]
    public int Index { get; set; }
    
    [JsonProperty(propertyName: "onOffEvents")]
    public List<NoteOnOffEvent> OnOffEvents { get; }
    
    [JsonProperty(propertyName: "controlKeyframes")]
    public List<ControlKeyframe> ControlKeyframes { get; }

    public TrackVisualsEventChannel() {
        OnOffEvents = new List<NoteOnOffEvent>();
        ControlKeyframes = new List<ControlKeyframe>();
    }
}