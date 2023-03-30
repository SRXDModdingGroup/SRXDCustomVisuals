using SRXDCustomVisuals.Core;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventPlayback {
    private TrackVisualsProject sequence;
    private int[] lastOnOffEventIndexPerColumn;
    private int[] lastControlKeyframeIndexPerColumn;

    public TrackVisualsEventPlayback() {
        lastOnOffEventIndexPerColumn = new int[Constants.IndexCount];
        lastControlKeyframeIndexPerColumn = new int[Constants.IndexCount];
        SetSequence(new TrackVisualsProject());
    }

    public void SetSequence(TrackVisualsProject sequence) {
        this.sequence = sequence;
        
        for (int i = 0; i < Constants.IndexCount; i++) {
            lastOnOffEventIndexPerColumn[i] = -1;
            lastControlKeyframeIndexPerColumn[i] = -1;
        }
    }

    public void Advance(long time) {
        AdvanceOnOffEvents(time);
        AdvanceControlCurves(time);
    }

    public void Jump(long time) {
        VisualsEventManager.ResetAll();
        JumpOnOffEvents(time);
        
        for (int i = 0; i < lastControlKeyframeIndexPerColumn.Length; i++)
            lastControlKeyframeIndexPerColumn[i] = -1;
        
        AdvanceControlCurves(time);
    }

    private void AdvanceOnOffEvents(long time) {
        var onOffEvents = sequence.OnOffEvents;
        
        for (int i = 0; i < onOffEvents.ColumnCount; i++) {
            var onOffEventsInColumn = onOffEvents.GetElementsInColumn(i);
            int startIndex = lastOnOffEventIndexPerColumn[i];
            int newIndex = startIndex;
            
            for (int j = startIndex + 1; j < onOffEventsInColumn.Count; j++) {
                var onOffEvent = onOffEventsInColumn[j];
                
                if (onOffEvent.Time > time)
                    break;
        
                switch (onOffEvent.Type) {
                    case OnOffEventType.On:
                        VisualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, i, onOffEvent.Value));
                        break;
                    case OnOffEventType.Off:
                        VisualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.Off, i, onOffEvent.Value));
                        break;
                    case OnOffEventType.OnOff:
                        VisualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, i, onOffEvent.Value));
                        VisualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.Off, i, onOffEvent.Value));
                        break;
                }

                newIndex = j;
            }

            lastOnOffEventIndexPerColumn[i] = newIndex;
        }
    }

    private void AdvanceControlCurves(long time) {
        var controlCurves = sequence.ControlCurves;
        
        for (int i = 0; i < controlCurves.ColumnCount; i++) {
            var keyframes = controlCurves.GetElementsInColumn(i);
            
            if (keyframes.Count == 0)
                continue;
            
            int index = lastControlKeyframeIndexPerColumn[i];

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

            VisualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.ControlChange, i, value));
            lastControlKeyframeIndexPerColumn[i] = index;
        }
    }

    private void JumpOnOffEvents(long time) {
        var onOffEvents = sequence.OnOffEvents;
        
        for (int i = 0; i < onOffEvents.ColumnCount; i++) {
            var onOffEventsInColumn = onOffEvents.GetElementsInColumn(i);
            int newIndex = -1;
            OnOffEvent eventToSend = null;

            for (int j = 0; j < onOffEventsInColumn.Count; j++) {
                var onOffEvent = onOffEventsInColumn[j];
                
                if (onOffEvent.Time > time)
                    break;

                if (onOffEvent.Type == OnOffEventType.On)
                    eventToSend = onOffEvent;
                else
                    eventToSend = null;

                newIndex = j;
            }

            lastOnOffEventIndexPerColumn[i] = newIndex;

            if (eventToSend != null)
                VisualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, i, eventToSend.Value));
        }
    }
}