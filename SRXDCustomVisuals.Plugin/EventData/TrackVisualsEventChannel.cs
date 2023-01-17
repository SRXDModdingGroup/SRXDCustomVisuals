using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventChannel {
    public byte Index { get; set; }
    
    public List<OnOffEvent> OnOffEvents { get; }
    
    public List<ControlCurve> ControlCurves { get; }

    public TrackVisualsEventChannel(byte index) {
        Index = index;
        OnOffEvents = new List<OnOffEvent>();
        ControlCurves = new List<ControlCurve>();
    }
}