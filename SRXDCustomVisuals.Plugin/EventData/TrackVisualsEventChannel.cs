using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventChannel {
    public List<OnOffEvent> OnOffEvents { get; }
    
    public ControlCurve[] ControlCurves { get; }

    public TrackVisualsEventChannel() {
        OnOffEvents = new List<OnOffEvent>();
        ControlCurves = new ControlCurve[256];
    }
}