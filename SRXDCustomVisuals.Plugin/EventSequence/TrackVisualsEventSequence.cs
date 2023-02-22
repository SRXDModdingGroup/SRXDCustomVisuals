using System;
using System.Collections.Generic;
using UnityEngine;

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

    public IReadOnlyList<Color32> Palette => palette;

    private string background;
    private List<Color32> palette;
    private List<OnOffEvent> onOffEvents;
    private List<ControlKeyframe>[] controlCurves;
    private UndoRedoStack undoRedoStack;
    private CompoundAction compoundAction;
    private bool dirty;

    public TrackVisualsEventSequence() {
        background = "";
        palette = new List<Color32>();
        onOffEvents = new List<OnOffEvent>();
        controlCurves = new List<ControlKeyframe>[Constants.IndexCount];

        for (int i = 0; i < controlCurves.Length; i++)
            controlCurves[i] = new List<ControlKeyframe>();

        undoRedoStack = new UndoRedoStack();
    }

    public TrackVisualsEventSequence(CustomVisualsInfo customVisualsInfo) {
        background = customVisualsInfo.Background;

        palette = new List<Color32>(customVisualsInfo.Palette.Count);

        foreach (var paletteColor in customVisualsInfo.Palette)
            palette.Add(new Color32((byte) paletteColor.Red, (byte) paletteColor.Green, (byte) paletteColor.Blue, 255));

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

    public void AddOnOffEvents(IList<OnOffEvent> toAdd, List<int> indices) {
        var toAddSorted = new List<OnOffEvent>(toAdd.Count);

        foreach (var onOffEvent in toAdd)
            toAddSorted.InsertSorted(onOffEvent);

        indices.Clear();

        foreach (var onOffEvent in toAddSorted)
            indices.InsertSorted(AddOnOffEvent(onOffEvent));
    }

    public void RemoveOnOffEvent(int index) {
        var onOffEvent = onOffEvents[index];
        
        onOffEvents.RemoveAt(index);
        compoundAction.AddAction(new UndoRedoAction(
            () => onOffEvents.Insert(index, onOffEvent),
            () => onOffEvents.RemoveAt(index)));
    }

    public void RemoveOnOffEvents(IList<int> indices) {
        var indicesSorted = new List<int>(indices.Count);

        foreach (int index in indices)
            indicesSorted.InsertSorted(index);

        for (int i = indicesSorted.Count - 1; i >= 0; i--)
            RemoveOnOffEvent(indicesSorted[i]);
    }
    
    public void ReplaceOnOffEvent(int index, OnOffEvent onOffEvent) {
        var oldEvent = onOffEvents[index];
        
        onOffEvents[index] = onOffEvent;
        compoundAction.AddAction(new UndoRedoAction(
            () => onOffEvents[index] = oldEvent,
            () => onOffEvents[index] = onOffEvent));
    }

    public void AddKeyframes(int column, IList<ControlKeyframe> toAdd, List<int> indices) {
        var toAddSorted = new List<ControlKeyframe>(toAdd.Count);

        foreach (var keyframe in toAdd)
            toAddSorted.InsertSorted(keyframe);

        indices.Clear();

        foreach (var keyframe in toAddSorted)
            indices.InsertSorted(AddKeyframe(column, keyframe));
    }

    public void RemoveKeyframe(int column, int index) {
        var keyframe = controlCurves[column][index];
        
        controlCurves[column].RemoveAt(index);
        compoundAction.AddAction(new UndoRedoAction(
            () => controlCurves[column].Insert(index, keyframe),
            () => controlCurves[column].RemoveAt(index)));
    }
    
    public void RemoveKeyframes(int column, IList<int> indices) {
        var indicesSorted = new List<int>(indices.Count);

        foreach (int index in indices)
            indicesSorted.InsertSorted(index);

        for (int i = indicesSorted.Count - 1; i >= 0; i--)
            RemoveKeyframe(column, indicesSorted[i]);
    }
    
    public void ReplaceKeyframe(int column, int index, ControlKeyframe keyframe) {
        var oldKeyframe = controlCurves[column][index];
        
        controlCurves[column][index] = keyframe;
        compoundAction.AddAction(new UndoRedoAction(
            () => controlCurves[column][index] = oldKeyframe,
            () => controlCurves[column][index] = keyframe));
    }

    public int AddOnOffEvent(OnOffEvent onOffEvent) {
        int index = onOffEvents.InsertSorted(onOffEvent);
        
        compoundAction.AddAction(new UndoRedoAction(
            () => onOffEvents.RemoveAt(index),
            () => onOffEvents.Insert(index, onOffEvent)));

        return index;
    }
    
    public int AddKeyframe(int column, ControlKeyframe keyframe) {
        int index = controlCurves[column].InsertSorted(keyframe);
        
        compoundAction.AddAction(new UndoRedoAction(
            () => controlCurves[column].RemoveAt(index),
            () => controlCurves[column].Insert(index, keyframe)));

        return index;
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
        var newPalette = new List<PaletteColor>(palette.Count);

        foreach (var color in palette)
            newPalette.Add(new PaletteColor(color.r, color.g, color.b));

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

        return new CustomVisualsInfo(Background, newPalette, events);
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