using System;
using SRXDCustomVisuals.Core;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventPlayback {
    private TrackVisualsEventSequence eventSequence = new();
    private int[] lastOnOffEventIndex = new int[256];
    private OnOffEvent[] onOffEventsToSend = new OnOffEvent[256];

    public void SetSequence(TrackVisualsEventSequence eventSequence) {
        VisualsEventManager.Instance.ResetAll();
        this.eventSequence = eventSequence;

        for (int i = 0; i < 256; i++) {
            lastOnOffEventIndex[i] = -1;
            onOffEventsToSend[i] = null;
        }
    }
    
    public void Advance(long time) {
        foreach (var channel in eventSequence.Channels)
            AdvanceChannel(channel, time);
    }

    public void Jump(long time) {
        VisualsEventManager.Instance.ResetAll();
        
        foreach (var channel in eventSequence.Channels)
            JumpChannel(channel, time);
    }

    private void AdvanceChannel(TrackVisualsEventChannel channel, long time) {
        var visualsEventManager = VisualsEventManager.Instance;
        byte channelIndex = channel.Index;
        var onOffEvents = channel.OnOffEvents;
        int startIndex = lastOnOffEventIndex[channelIndex];
        int newIndex = startIndex;

        for (int i = startIndex + 1; i < onOffEvents.Count; i++) {
            var onOffEvent = onOffEvents[i];
                
            if (onOffEvent.Time > time)
                break;
        
            switch (onOffEvent.Type) {
                case OnOffEventType.On:
                    visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, channelIndex, onOffEvent.Index, onOffEvent.Value));
                    break;
                case OnOffEventType.Off:
                    visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.Off, channelIndex, onOffEvent.Index, onOffEvent.Value));
                    break;
                case OnOffEventType.OnOff:
                    visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, channelIndex, onOffEvent.Index, onOffEvent.Value));
                    visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.Off, channelIndex, onOffEvent.Index, onOffEvent.Value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            newIndex = i;
        }
        
        lastOnOffEventIndex[channelIndex] = newIndex;
    }

    private void JumpChannel(TrackVisualsEventChannel channel, long time) {
        var visualsEventManager = VisualsEventManager.Instance;
        byte channelIndex = channel.Index;
        var onOffEvents = channel.OnOffEvents;
        int newIndex = -1;

        for (int i = 0; i < onOffEvents.Count; i++) {
            var onOffEvent = onOffEvents[i];
                
            if (onOffEvent.Time > time)
                break;

            if (onOffEvent.Type == OnOffEventType.On)
                onOffEventsToSend[onOffEvent.Index] = onOffEvent;
            else
                onOffEventsToSend[onOffEvent.Index] = null;
            
            newIndex = i;
        }

        for (int i = 0; i < 256; i++) {
            var onOffEvent = onOffEventsToSend[i];
            
            if (onOffEvent == null)
                continue;
            
            visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, channelIndex, onOffEvent.Index, onOffEvent.Value));
            onOffEventsToSend[i] = null;
        }
        
        lastOnOffEventIndex[channelIndex] = newIndex;
    }
}