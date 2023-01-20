using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventSequence {
    public string Background { get; set; }
    
    public List<OnOffEvent> OnOffEvents { get; }
    
    public ControlCurve[] ControlCurves { get; }

    public TrackVisualsEventSequence() {
        Background = "";
        OnOffEvents = new List<OnOffEvent>();
        ControlCurves = new ControlCurve[256];

        for (int i = 0; i < 256; i++)
            ControlCurves[i] = new ControlCurve();
    }

    public TrackVisualsEventSequence(CustomVisualsInfo customVisualsInfo) {
        Background = customVisualsInfo.Background;
        OnOffEvents = new List<OnOffEvent>();
        ControlCurves = new ControlCurve[256];

        for (int i = 0; i < 256; i++)
            ControlCurves[i] = new ControlCurve();
        
        foreach (var visualsEvent in customVisualsInfo.Events) {
            if (visualsEvent.Type == TrackVisualsEventType.ControlKeyframe)
                ControlCurves[visualsEvent.Index].Keyframes.Add(new ControlKeyframe(visualsEvent.Time, visualsEvent.KeyframeType, visualsEvent.Value));
            else
                OnOffEvents.Add(new OnOffEvent(visualsEvent.Time, ToOnOffEventType(visualsEvent.Type), visualsEvent.Index, visualsEvent.Value));
        }
    }

    public CustomVisualsInfo ToCustomVisualsInfo() {
        var events = new List<TrackVisualsEvent>();

        foreach (var onOffEvent in OnOffEvents) {
            events.InsertSorted(new TrackVisualsEvent(
                onOffEvent.Time,
                ToTrackVisualsEventType(onOffEvent.Type),
                ControlKeyframeType.Constant,
                onOffEvent.Index,
                onOffEvent.Value));
        }

        for (int i = 0; i < 256; i++) {
            var controlCurve = ControlCurves[i];
                
            foreach (var controlKeyframe in controlCurve.Keyframes) {
                events.InsertSorted(new TrackVisualsEvent(
                    controlKeyframe.Time,
                    TrackVisualsEventType.ControlKeyframe,
                    controlKeyframe.Type,
                    i,
                    controlKeyframe.Value));
            }
        }

        return new CustomVisualsInfo(Background, events);
    }

    private static OnOffEventType ToOnOffEventType(TrackVisualsEventType type) => type switch {
        TrackVisualsEventType.On => OnOffEventType.On,
        TrackVisualsEventType.Off => OnOffEventType.Off,
        TrackVisualsEventType.OnOff => OnOffEventType.OnOff,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    private static TrackVisualsEventType ToTrackVisualsEventType(OnOffEventType type) => type switch {
        OnOffEventType.On => TrackVisualsEventType.On,
        OnOffEventType.Off => TrackVisualsEventType.Off,
        OnOffEventType.OnOff => TrackVisualsEventType.OnOff,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}