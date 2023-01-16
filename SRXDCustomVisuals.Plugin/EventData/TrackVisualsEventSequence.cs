using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventSequence {
    public List<TrackVisualsEventChannel> Channels { get; }

    public TrackVisualsEventSequence() {
        Channels = new List<TrackVisualsEventChannel>();
    }

    public TrackVisualsEventSequence(List<TrackVisualsEvent> events) {
        var channelsByIndex = new Dictionary<int, TrackVisualsEventChannel>();
        var controlCurvesByIndexByChannel = new Dictionary<int, Dictionary<int, ControlCurve>>();
        
        Channels = new List<TrackVisualsEventChannel>();
        
        foreach (var visualsEvent in events) {
            if (!channelsByIndex.TryGetValue(visualsEvent.Channel, out var channel)) {
                channel = new TrackVisualsEventChannel((byte) visualsEvent.Channel);
                channelsByIndex.Add(visualsEvent.Channel, channel);
                Channels.Add(channel);
            }

            if (visualsEvent.Type != TrackVisualsEventType.ControlKeyframe) {
                channel.OnOffEvents.Add(new OnOffEvent(visualsEvent.Time, ToOnOffEventType(visualsEvent.Type), (byte) visualsEvent.Index, (byte) visualsEvent.Value));
                
                continue;
            }
            
            var controlCurves = channel.ControlCurves;
            
            if (!controlCurvesByIndexByChannel.TryGetValue(visualsEvent.Channel, out var controlCurvesByIndex)) {
                controlCurvesByIndex = new Dictionary<int, ControlCurve>();
                controlCurvesByIndexByChannel.Add(visualsEvent.Channel, controlCurvesByIndex);
            }

            if (!controlCurvesByIndex.TryGetValue(visualsEvent.Index, out var controlCurve)) {
                controlCurve = new ControlCurve(visualsEvent.Index);
                controlCurvesByIndex.Add(visualsEvent.Index, controlCurve);
                controlCurves.Add(controlCurve);
            }
            
            controlCurve.Keyframes.Add(new ControlKeyframe(visualsEvent.Time, visualsEvent.KeyframeType, (byte) visualsEvent.Value));
        }
    }

    public List<TrackVisualsEvent> ToVisualsEvents() {
        var visualsEvents = new List<TrackVisualsEvent>();

        foreach (var channel in Channels) {
            foreach (var onOffEvent in channel.OnOffEvents) {
                InsertEvent(new TrackVisualsEvent(
                    onOffEvent.Time,
                    ToTrackVisualsEventType(onOffEvent.Type),
                    ControlKeyframeType.Constant,
                    channel.Index,
                    onOffEvent.Index,
                    onOffEvent.Value));
            }

            foreach (var controlCurve in channel.ControlCurves) {
                foreach (var controlKeyframe in controlCurve.Keyframes) {
                    InsertEvent(new TrackVisualsEvent(
                        controlKeyframe.Time,
                        TrackVisualsEventType.ControlKeyframe,
                        controlKeyframe.Type,
                        channel.Index,
                        controlCurve.Index,
                        controlKeyframe.Value));
                }
            }
        }

        return visualsEvents;

        void InsertEvent(TrackVisualsEvent visualsEvent) {
            int index = visualsEvents.BinarySearch(visualsEvent);

            if (index < 0)
                index = ~index;

            while (index < visualsEvents.Count && visualsEvent.Time <= visualsEvents[index].Time)
                index++;

            visualsEvents.Insert(index, visualsEvent);
        }
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