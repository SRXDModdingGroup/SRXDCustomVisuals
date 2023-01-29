using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventSequence {
    public int ColumnCount => Constants.IndexCount;
    
    public string Background {
        get => background;
        set {
            if (value == background)
                return;
        
            background = value;
            dirty = true;
        }
    }
    
    private string background;
    private List<OnOffEvent> onOffEvents;
    private List<ControlKeyframe>[] controlCurves;
    private UndoRedoStack undoRedoStack;
    private CompoundAction compoundAction;
    private bool dirty;

    public TrackVisualsEventSequence() {
        background = "";
        onOffEvents = new List<OnOffEvent>();
        controlCurves = new List<ControlKeyframe>[Constants.IndexCount];

        for (int i = 0; i < controlCurves.Length; i++)
            controlCurves[i] = new List<ControlKeyframe>();

        undoRedoStack = new UndoRedoStack();
    }

    public TrackVisualsEventSequence(CustomVisualsInfo customVisualsInfo) {
        background = customVisualsInfo.Background;
        onOffEvents = new List<OnOffEvent>();
        controlCurves = new List<ControlKeyframe>[Constants.IndexCount];

        for (int i = 0; i < controlCurves.Length; i++)
            controlCurves[i] = new List<ControlKeyframe>();
        
        foreach (var visualsEvent in customVisualsInfo.Events) {
            if (visualsEvent.Type == TrackVisualsEventType.ControlKeyframe)
                controlCurves[visualsEvent.Index].Add(new ControlKeyframe(visualsEvent.Time, visualsEvent.KeyframeType, visualsEvent.Value));
            else
                onOffEvents.Add(new OnOffEvent(visualsEvent.Time, ToOnOffEventType(visualsEvent.Type), visualsEvent.Index, visualsEvent.Value));
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
        onOffEvents.Insert(index, onOffEvent);
        compoundAction.AddAction(new UndoRedoAction(
            () => onOffEvents.RemoveAt(index),
            () => onOffEvents.Insert(index, onOffEvent)));
    }

    public void RemoveOnOffEvent(int index) {
        var onOffEvent = onOffEvents[index];
        
        onOffEvents.RemoveAt(index);
        compoundAction.AddAction(new UndoRedoAction(
            () => onOffEvents.Insert(index, onOffEvent),
            () => onOffEvents.RemoveAt(index)));
    }
    
    public void ReplaceOnOffEvent(int index, OnOffEvent onOffEvent) {
        var oldEvent = onOffEvents[index];
        
        onOffEvents[index] = onOffEvent;
        compoundAction.AddAction(new UndoRedoAction(
            () => onOffEvents[index] = oldEvent,
            () => onOffEvents[index] = onOffEvent));
    }
    
    public void AddKeyframe(int column, int index, ControlKeyframe keyframe) {
        controlCurves[column].Insert(index, keyframe);
        compoundAction.AddAction(new UndoRedoAction(
            () => controlCurves[column].RemoveAt(index),
            () => controlCurves[column].Insert(index, keyframe)));
    }

    public void RemoveKeyframe(int column, int index) {
        var keyframe = controlCurves[column][index];
        
        controlCurves[column].RemoveAt(index);
        compoundAction.AddAction(new UndoRedoAction(
            () => controlCurves[column].Insert(index, keyframe),
            () => controlCurves[column].RemoveAt(index)));
    }
    
    public void ReplaceKeyframe(int column, int index, ControlKeyframe keyframe) {
        var oldKeyframe = controlCurves[column][index];
        
        controlCurves[column][index] = keyframe;
        compoundAction.AddAction(new UndoRedoAction(
            () => controlCurves[column][index] = oldKeyframe,
            () => controlCurves[column][index] = keyframe));
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

    public IReadOnlyList<OnOffEvent> GetOnOffEvents() => onOffEvents;

    public IReadOnlyList<ControlKeyframe> GetKeyframes(int column) => controlCurves[column];

    public CustomVisualsInfo ToCustomVisualsInfo() {
        var events = new List<TrackVisualsEvent>();

        foreach (var onOffEvent in onOffEvents) {
            events.InsertSorted(new TrackVisualsEvent(
                onOffEvent.Time,
                ToTrackVisualsEventType(onOffEvent.Type),
                ControlKeyframeType.Constant,
                onOffEvent.Index,
                onOffEvent.Value));
        }

        for (int i = 0; i < controlCurves.Length; i++) {
            var keyframes = controlCurves[i];
                
            foreach (var keyframe in keyframes) {
                events.InsertSorted(new TrackVisualsEvent(
                    keyframe.Time,
                    TrackVisualsEventType.ControlKeyframe,
                    keyframe.Type,
                    i,
                    keyframe.Value));
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