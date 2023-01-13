using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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

            switch (visualsEvent.Type) {
                case TrackVisualsEventType.On:
                    channel.OnOffEvents.Add(new OnOffEvent(visualsEvent.Time, true, (byte) visualsEvent.Index, (byte) visualsEvent.Value));
                    break;
                case TrackVisualsEventType.Off:
                    channel.OnOffEvents.Add(new OnOffEvent(visualsEvent.Time, false, (byte) visualsEvent.Index, (byte) visualsEvent.Value));
                    break;
                case TrackVisualsEventType.ControlKeyframe:
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public List<TrackVisualsEvent> ToVisualsEvents() {
        var visualsEvents = new List<TrackVisualsEvent>();

        foreach (var channel in Channels) {
            foreach (var onOffEvent in channel.OnOffEvents) {
                InsertEvent(new TrackVisualsEvent(
                    onOffEvent.Time,
                    onOffEvent.On ? TrackVisualsEventType.On : TrackVisualsEventType.Off,
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
}