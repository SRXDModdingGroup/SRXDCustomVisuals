using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventSequence {
    public string Background {
        get => background;
        set {
            if (value == background)
                return;
        
            background = value;
            dirty = true;
        }
    }

    public List<OnOffEvent> OnOffEvents { get; }
    
    public ControlCurve[] ControlCurves { get; }
    
    private string background;
    private UndoRedoStack undoRedoStack;
    private CompoundAction compoundAction;
    private bool dirty;

    public TrackVisualsEventSequence() {
        background = "";
        OnOffEvents = new List<OnOffEvent>();
        ControlCurves = new ControlCurve[Constants.IndexCount];

        for (int i = 0; i < ControlCurves.Length; i++)
            ControlCurves[i] = new ControlCurve();

        undoRedoStack = new UndoRedoStack();
    }

    public TrackVisualsEventSequence(CustomVisualsInfo customVisualsInfo) {
        background = customVisualsInfo.Background;
        OnOffEvents = new List<OnOffEvent>();
        ControlCurves = new ControlCurve[Constants.IndexCount];

        for (int i = 0; i < ControlCurves.Length; i++)
            ControlCurves[i] = new ControlCurve();
        
        foreach (var visualsEvent in customVisualsInfo.Events) {
            if (visualsEvent.Type == TrackVisualsEventType.ControlKeyframe)
                ControlCurves[visualsEvent.Index].Keyframes.Add(new ControlKeyframe(visualsEvent.Time, visualsEvent.KeyframeType, visualsEvent.Value));
            else
                OnOffEvents.Add(new OnOffEvent(visualsEvent.Time, ToOnOffEventType(visualsEvent.Type), visualsEvent.Index, visualsEvent.Value));
        }

        undoRedoStack = new UndoRedoStack();
    }

    public void BeginEdit() {
        compoundAction = new CompoundAction();
        dirty = false;
    }

    public void Undo() {
        if (!undoRedoStack.CanUndo)
            return;
        
        undoRedoStack.Undo();
        dirty = true;
    }

    public void Redo() {
        if (!undoRedoStack.CanRedo)
            return;
        
        undoRedoStack.Redo();
        dirty = true;
    }

    public void AddOnOffEvent(int index, OnOffEvent onOffEvent) {
        OnOffEvents.Insert(index, onOffEvent);
        compoundAction.AddAction(new UndoRedoAction(
            () => OnOffEvents.RemoveAt(index),
            () => OnOffEvents.Insert(index, onOffEvent)));
    }

    public void RemoveOnOffEvent(int index) {
        var onOffEvent = OnOffEvents[index];
        
        OnOffEvents.RemoveAt(index);
        compoundAction.AddAction(new UndoRedoAction(
            () => OnOffEvents.Insert(index, onOffEvent),
            () => OnOffEvents.RemoveAt(index)));
    }
    
    public void ReplaceOnOffEvent(int index, OnOffEvent onOffEvent) {
        var oldEvent = OnOffEvents[index];
        
        OnOffEvents[index] = onOffEvent;
        compoundAction.AddAction(new UndoRedoAction(
            () => OnOffEvents[index] = oldEvent,
            () => OnOffEvents[index] = onOffEvent));
    }
    
    public void AddKeyframe(int column, int index, ControlKeyframe keyframe) {
        ControlCurves[column].Keyframes.Insert(index, keyframe);
        compoundAction.AddAction(new UndoRedoAction(
            () => ControlCurves[column].Keyframes.RemoveAt(index),
            () => ControlCurves[column].Keyframes.Insert(index, keyframe)));
    }

    public void RemoveKeyframe(int column, int index) {
        var keyframe = ControlCurves[column].Keyframes[index];
        
        ControlCurves[column].Keyframes.RemoveAt(index);
        compoundAction.AddAction(new UndoRedoAction(
            () => ControlCurves[column].Keyframes.Insert(index, keyframe),
            () => ControlCurves[column].Keyframes.RemoveAt(index)));
    }
    
    public void ReplaceKeyframe(int column, int index, ControlKeyframe keyframe) {
        var oldKeyframe = ControlCurves[column].Keyframes[index];
        
        ControlCurves[column].Keyframes[index] = keyframe;
        compoundAction.AddAction(new UndoRedoAction(
            () => ControlCurves[column].Keyframes[index] = oldKeyframe,
            () => ControlCurves[column].Keyframes[index] = keyframe));
    }

    public bool EndEdit() {
        bool wasDirty = dirty;
        
        if (compoundAction.Count > 0) {
            undoRedoStack.AddAction(compoundAction);
            wasDirty = true;
        }
        
        compoundAction = null;
        dirty = false;

        return wasDirty;
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

        for (int i = 0; i < ControlCurves.Length; i++) {
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
        _ => default
    };

    private static TrackVisualsEventType ToTrackVisualsEventType(OnOffEventType type) => type switch {
        OnOffEventType.On => TrackVisualsEventType.On,
        OnOffEventType.Off => TrackVisualsEventType.Off,
        OnOffEventType.OnOff => TrackVisualsEventType.OnOff,
        _ => default
    };
}