using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventSequence {
    public List<OnOffEvent> OnOffEvents { get; }
    
    public ControlCurve[] ControlCurves { get; }

    public TrackVisualsEventSequence() {
        OnOffEvents = new List<OnOffEvent>();
        ControlCurves = new ControlCurve[256];

        for (int i = 0; i < 256; i++)
            ControlCurves[i] = new ControlCurve();
    }

    public TrackVisualsEventSequence(List<TrackVisualsEvent> events) {
        OnOffEvents = new List<OnOffEvent>();
        ControlCurves = new ControlCurve[256];
        
        foreach (var visualsEvent in events) {
            if (visualsEvent.Type == TrackVisualsEventType.ControlKeyframe)
                ControlCurves[visualsEvent.Index].Keyframes.Add(new ControlKeyframe(visualsEvent.Time, visualsEvent.KeyframeType, visualsEvent.Value));
            else
                OnOffEvents.Add(new OnOffEvent(visualsEvent.Time, ToOnOffEventType(visualsEvent.Type), visualsEvent.Index, visualsEvent.Value));
        }
    }

    public List<TrackVisualsEvent> ToVisualsEvents() {
        var visualsEvents = new List<TrackVisualsEvent>();

        foreach (var onOffEvent in OnOffEvents) {
            visualsEvents.InsertSorted(new TrackVisualsEvent(
                onOffEvent.Time,
                ToTrackVisualsEventType(onOffEvent.Type),
                ControlKeyframeType.Constant,
                onOffEvent.Index,
                onOffEvent.Value));
        }

        for (int j = 0; j < 256; j++) {
            var controlCurve = ControlCurves[j];
                
            foreach (var controlKeyframe in controlCurve.Keyframes) {
                visualsEvents.InsertSorted(new TrackVisualsEvent(
                    controlKeyframe.Time,
                    TrackVisualsEventType.ControlKeyframe,
                    controlKeyframe.Type,
                    j,
                    controlKeyframe.Value));
            }
        }

        return visualsEvents;
    }

    private static OnOffEventType ToOnOffEventType(TrackVisualsEventType type) => type switch {
        TrackVisualsEventType.On => OnOffEventType.On,
        TrackVisualsEventType.Off => OnOffEventType.Off,
        TrackVisualsEventType.OnOff => OnOffEventType.Off,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    private static TrackVisualsEventType ToTrackVisualsEventType(OnOffEventType type) => type switch {
        OnOffEventType.On => TrackVisualsEventType.On,
        OnOffEventType.Off => TrackVisualsEventType.Off,
        OnOffEventType.OnOff => TrackVisualsEventType.OnOff,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}