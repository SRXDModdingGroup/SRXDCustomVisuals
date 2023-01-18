using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventSequence {
    public TrackVisualsEventChannel[] Channels { get; }

    public TrackVisualsEventSequence() {
        Channels = new TrackVisualsEventChannel[256];

        for (int i = 0; i < 256; i++)
            Channels[i] = new TrackVisualsEventChannel();
    }

    public TrackVisualsEventSequence(List<TrackVisualsEvent> events) {
        Channels = new TrackVisualsEventChannel[256];

        for (int i = 0; i < 256; i++)
            Channels[i] = new TrackVisualsEventChannel();
        
        foreach (var visualsEvent in events) {
            var channel = Channels[visualsEvent.Index];

            if (visualsEvent.Type == TrackVisualsEventType.ControlKeyframe)
                channel.ControlCurves[visualsEvent.Index].Keyframes.Add(new ControlKeyframe(visualsEvent.Time, visualsEvent.KeyframeType, (byte) visualsEvent.Value));
            else
                channel.OnOffEvents.Add(new OnOffEvent(visualsEvent.Time, ToOnOffEventType(visualsEvent.Type), (byte) visualsEvent.Index, (byte) visualsEvent.Value));
        }
    }

    public List<TrackVisualsEvent> ToVisualsEvents() {
        var visualsEvents = new List<TrackVisualsEvent>();

        for (int i = 0; i < Channels.Length; i++) {
            var channel = Channels[i];
            
            foreach (var onOffEvent in channel.OnOffEvents) {
                visualsEvents.InsertSorted(new TrackVisualsEvent(
                    onOffEvent.Time,
                    ToTrackVisualsEventType(onOffEvent.Type),
                    ControlKeyframeType.Constant,
                    i,
                    onOffEvent.Index,
                    onOffEvent.Value));
            }

            for (int j = 0; j < channel.ControlCurves.Length; j++) {
                var controlCurve = channel.ControlCurves[j];
                
                foreach (var controlKeyframe in controlCurve.Keyframes) {
                    visualsEvents.InsertSorted(new TrackVisualsEvent(
                        controlKeyframe.Time,
                        TrackVisualsEventType.ControlKeyframe,
                        controlKeyframe.Type,
                        i,
                        j,
                        controlKeyframe.Value));
                }
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