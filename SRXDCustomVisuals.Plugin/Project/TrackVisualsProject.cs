using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsProject {
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

    public IReadOnlySequence<OnOffEvent> OnOffEvents => onOffEvents;

    public IReadOnlySequence<ControlKeyframe> ControlCurves => controlCurves;

    private string background;
    private Color32[] palette;
    private Sequence<OnOffEvent> onOffEvents;
    private Sequence<ControlKeyframe> controlCurves;
    private UndoRedoStack undoRedoStack;
    private CompoundAction compoundAction;
    private bool dirty;

    public TrackVisualsProject() {
        background = "";
        palette = new Color32[Constants.PaletteSize];

        for (int i = 0; i < Constants.PaletteSize; i++)
            palette[i] = new Color32(255, 255, 255, 255);

        onOffEvents = new Sequence<OnOffEvent>(ColumnCount);
        controlCurves = new Sequence<ControlKeyframe>(ColumnCount);
        undoRedoStack = new UndoRedoStack();
    }

    public TrackVisualsProject(CustomVisualsInfo customVisualsInfo) {
        background = customVisualsInfo.Background;
        palette = new Color32[Constants.PaletteSize];

        var fromPalette = customVisualsInfo.Palette;

        for (int i = 0; i < Constants.PaletteSize; i++) {
            if (i < fromPalette.Count) {
                var fromColor = fromPalette[i];

                palette[i] = new Color32((byte) fromColor.Red, (byte) fromColor.Green, (byte) fromColor.Blue, 255);
            }
            else
                palette[i] = new Color32(255, 255, 255, 255);
        }

        onOffEvents = new Sequence<OnOffEvent>(ColumnCount);
        controlCurves = new Sequence<ControlKeyframe>(ColumnCount);
        
        foreach (var visualsEvent in customVisualsInfo.Events) {
            if (visualsEvent.Type == TrackVisualsEventType.ControlKeyframe)
                controlCurves.AddElement(visualsEvent.Index, new ControlKeyframe(visualsEvent.Time, visualsEvent.KeyframeType, visualsEvent.Value));
            else
                onOffEvents.AddElement(visualsEvent.Index, new OnOffEvent(visualsEvent.Time, ToOnOffEventType(visualsEvent.Type), visualsEvent.Value));
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

    public void SetPaletteColor(int index, Color32 color) {
        if (index < 0 || index >= palette.Length || Util.ColorEquals(color, palette[index]))
            return;
        
        palette[index] = color;
        dirty = true;
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

    public SequenceEditHandle<OnOffEvent> GetHandleForOnOffEvents => new(onOffEvents, compoundAction);

    public SequenceEditHandle<ControlKeyframe> GetHandleForControlCurves => new(controlCurves, compoundAction);

    public CustomVisualsInfo ToCustomVisualsInfo() {
        var newPalette = new List<PaletteColor>(Constants.PaletteSize);

        foreach (var color in palette)
            newPalette.Add(new PaletteColor(color));

        var events = new List<TrackVisualsEvent>();

        for (int i = 0; i < onOffEvents.ColumnCount; i++) {
            foreach (var onOffEvent in onOffEvents.GetElementsInColumn(i)) {
                events.InsertSorted(new TrackVisualsEvent(
                    onOffEvent.Time,
                    ToTrackVisualsEventType(onOffEvent.Type),
                    ControlKeyframeType.Constant,
                    i,
                    onOffEvent.Value));
            }
        }

        for (int i = 0; i < controlCurves.ColumnCount; i++) {
            foreach (var keyframe in controlCurves.GetElementsInColumn(i)) {
                events.InsertSorted(new TrackVisualsEvent(
                    keyframe.Time,
                    TrackVisualsEventType.ControlKeyframe,
                    keyframe.Type,
                    i,
                    keyframe.Value));
            }
        }

        return new CustomVisualsInfo(Background, newPalette, new Dictionary<string, string>(), events);
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