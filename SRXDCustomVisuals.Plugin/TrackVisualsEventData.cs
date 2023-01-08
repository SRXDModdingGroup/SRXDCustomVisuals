using System.Collections.Generic;
using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventData {
    [JsonProperty(propertyName: "channels")]
    public List<TrackVisualsEventChannel> Channels { get; }

    public TrackVisualsEventData() {
        Channels = new List<TrackVisualsEventChannel>();
    }
}