using System.Collections.Generic;
using SRXDCustomVisuals.Core;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventPlayback {
    private TrackVisualsEventSequence eventSequence;
    private bool[,] activeNoteOns = new bool[256, 256];
    private int[] lastOnOffEventPerChannel = new int[256];
    private OnOffEvent[] onOffsToSend = new OnOffEvent[256];
    
    public TrackVisualsEventPlayback(TrackVisualsEventSequence eventSequence) {
        this.eventSequence = eventSequence;
    }

    public void Advance(long time) {
        foreach (var channel in eventSequence.Channels) {
            if (AdvanceChannel(channel, time))
                SendOnOffEvents(channel);
        }
    }

    public void Jump(long time) {
        foreach (var channel in eventSequence.Channels) {
            if (ResetChannel(channel) | AdvanceChannel(channel, time))
                SendOnOffEvents(channel);
        }
    }

    public void Reset() {
        foreach (var channel in eventSequence.Channels) {
            if (ResetChannel(channel))
                SendOnOffEvents(channel);
        }
    }

    private bool AdvanceChannel(TrackVisualsEventChannel channel, long time) {
        byte channelIndex = channel.Index;
        var onOffEvents = channel.OnOffEvents;
        int startIndex = lastOnOffEventPerChannel[channelIndex];
        int newIndex = startIndex;

        for (int i = startIndex + 1; i < onOffEvents.Count; i++) {
            var onOffEvent = onOffEvents[i];
                
            if (onOffEvent.Time > time)
                break;

            onOffsToSend[onOffEvent.Index] = onOffEvent;
            newIndex = i;
        }

        if (newIndex != startIndex)
            return false;
        
        lastOnOffEventPerChannel[channelIndex] = newIndex;

        return true;
    }
    
    private bool ResetChannel(TrackVisualsEventChannel channel) {
        byte channelIndex = channel.Index;
        bool any = false;
        
        for (int i = 0; i < 256; i++) {
            if (!activeNoteOns[channelIndex, i])
                continue;
            
            onOffsToSend[i] = new OnOffEvent(0, false, (byte) i, 0);
            any = true;
        }

        lastOnOffEventPerChannel[channelIndex] = -1;

        return any;
    }

    private void SendOnOffEvents(TrackVisualsEventChannel channel) {
        byte channelIndex = channel.Index;
        var visualsEventManager = VisualsEventManager.Instance;
        
        for (int i = 0; i < 256; i++) {
            var onOffEvent = onOffsToSend[i];
                
            if (onOffEvent == null)
                continue;
                
            visualsEventManager.SendEvent(new VisualsEvent(onOffEvent.On ? VisualsEventType.On : VisualsEventType.Off, channelIndex, onOffEvent.Index, onOffEvent.Value));
            onOffsToSend[i] = null;
            activeNoteOns[channelIndex, onOffEvent.Index] = onOffEvent.On;
        }
    }
}