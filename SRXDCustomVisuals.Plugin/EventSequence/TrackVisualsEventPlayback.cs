using System;
using SRXDCustomVisuals.Core;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventPlayback {
    private TrackVisualsEventSequence eventSequence = new();
    private int lastOnOffEventIndex = -1;
    private OnOffEvent[] onOffEventsToSend = new OnOffEvent[256];
    private bool playing;
    private long lastTime;

    public void SetSequence(TrackVisualsEventSequence eventSequence) {
        VisualsEventManager.Instance.ResetAll();
        this.eventSequence = eventSequence;
    }

    public void Play(long time) {
        playing = true;
        
        if (time != lastTime)
            Jump(time);
    }

    public void Pause() => playing = false;

    public void Advance(long time) {
        if (!playing || time == lastTime)
            return;

        if (time < lastTime) {
            Jump(time);
            
            return;
        }

        var visualsEventManager = VisualsEventManager.Instance;
        var onOffEvents = eventSequence.OnOffEvents;
        int startIndex = lastOnOffEventIndex;
        int newIndex = startIndex;

        for (int i = startIndex + 1; i < onOffEvents.Count; i++) {
            var onOffEvent = onOffEvents[i];
                
            if (onOffEvent.Time > time)
                break;
        
            switch (onOffEvent.Type) {
                case OnOffEventType.On:
                    visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, onOffEvent.Index, onOffEvent.Value));
                    break;
                case OnOffEventType.Off:
                    visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.Off, onOffEvent.Index, onOffEvent.Value));
                    break;
                case OnOffEventType.OnOff:
                    visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, onOffEvent.Index, onOffEvent.Value));
                    visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.Off, onOffEvent.Index, onOffEvent.Value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            newIndex = i;
        }
        
        lastOnOffEventIndex = newIndex;
        lastTime = time;
    }

    public void Jump(long time) {
        if (!playing || time == lastTime)
            return;

        var visualsEventManager = VisualsEventManager.Instance;
        var onOffEvents = eventSequence.OnOffEvents;
        int newIndex = -1;
        
        visualsEventManager.ResetAll();

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
            
            visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, onOffEvent.Index, onOffEvent.Value));
            onOffEventsToSend[i] = null;
        }
        
        lastOnOffEventIndex = newIndex;
        lastTime = time;
    }
}