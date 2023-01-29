using System;
using SRXDCustomVisuals.Core;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventPlayback {
    private const long MIN_JUMP_INTERVAL = 2000L;
    
    private TrackVisualsEventSequence sequence;
    private int lastOnOffEventIndex;
    private OnOffEvent[] onOffEventsToSend;
    private int[] lastControlKeyframeIndex;
    private bool playing;
    private long lastTime;

    public TrackVisualsEventPlayback() {
        onOffEventsToSend = new OnOffEvent[Constants.IndexCount];
        lastControlKeyframeIndex = new int[Constants.IndexCount];
        SetSequence(new TrackVisualsEventSequence());
    }

    public void SetSequence(TrackVisualsEventSequence sequence) {
        VisualsEventManager.Instance.ResetAll();
        this.sequence = sequence;
        lastOnOffEventIndex = -1;
        
        for (int i = 0; i < onOffEventsToSend.Length; i++) {
            onOffEventsToSend[i] = null;
            lastControlKeyframeIndex[i] = -1;
        }

        lastTime = -1L;
    }

    public void Play(long time) {
        playing = true;
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
        var onOffEvents = sequence.GetOnOffEvents();
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
            }

            newIndex = i;
        }
        
        lastOnOffEventIndex = newIndex;
        lastTime = time;
        ProcessControlCurves(time);
    }

    public void Jump(long time, bool force = false) {
        if (!force) {
            if (!playing || time == lastTime)
                return;

            if (lastTime >= 0L) {
                if (time < lastTime && lastTime - time < MIN_JUMP_INTERVAL)
                    return;

                if (time > lastTime && time - lastTime < MIN_JUMP_INTERVAL) {
                    Advance(time);

                    return;
                }
            }
        }

        var visualsEventManager = VisualsEventManager.Instance;
        var onOffEvents = sequence.GetOnOffEvents();
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

        for (int i = 0; i < onOffEventsToSend.Length; i++) {
            var onOffEvent = onOffEventsToSend[i];
            
            if (onOffEvent == null)
                continue;
            
            visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, onOffEvent.Index, onOffEvent.Value));
            onOffEventsToSend[i] = null;
        }
        
        lastOnOffEventIndex = newIndex;
        lastTime = time;
        
        for (int i = 0; i < lastControlKeyframeIndex.Length; i++)
            lastControlKeyframeIndex[i] = -1;
        
        ProcessControlCurves(time);
    }

    private void ProcessControlCurves(long time) {
        var visualsEventManager = VisualsEventManager.Instance;

        for (int i = 0; i < sequence.ColumnCount; i++) {
            var keyframes = sequence.GetKeyframes(i);
            
            if (keyframes.Count == 0)
                continue;
            
            int index = lastControlKeyframeIndex[i];

            for (int j = index + 1; j < keyframes.Count; j++) {
                var keyframe = keyframes[j];

                if (keyframe.Time > time)
                    break;

                index = j;
            }

            float value;

            if (index < 0)
                value = keyframes[0].Value;
            else if (index >= keyframes.Count - 1)
                value = keyframes[keyframes.Count - 1].Value;
            else
                value = ControlKeyframe.Interpolate(keyframes[index], keyframes[index + 1], time);

            visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.ControlChange, i, value));
            lastControlKeyframeIndex[i] = index;
        }
    }
}