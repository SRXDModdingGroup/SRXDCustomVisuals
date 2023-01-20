using System;
using System.Collections.Generic;
using UnityEngine;
using Input = UnityEngine.Input;

namespace SRXDCustomVisuals.Plugin;

public class SequenceEditor : MonoBehaviour {
    private const float TIME_TO_TICK = 100000f;
    private const float TICK_TO_TIME = 0.00001f;
    private const float WINDOW_WIDTH = 800;
    private const float WINDOW_HEIGHT = 600;
    private const int COLUMN_COUNT = 16;
    private const long SELECTION_EPSILON = 1000L;
    
    public bool Visible { get; set; }

    public bool Dirty => true;

    private SequenceEditorState state;
    private SequenceRenderer renderer;
    private PlayState playState;
    private TrackVisualsEventSequence sequence;
    private List<OnOffEvent> onOffEventClipboard;
    private bool selecting;

    private void Awake() {
        state = new SequenceEditorState();
        renderer = new SequenceRenderer(WINDOW_WIDTH, WINDOW_HEIGHT, COLUMN_COUNT);
        sequence = new TrackVisualsEventSequence();
        onOffEventClipboard = new List<OnOffEvent>();
    }

    private void OnGUI() {
        if (Visible)
            renderer.Render(new RenderInfo(playState, state, sequence));
    }

    public void Init(TrackVisualsEventSequence sequence, PlayState playState) {
        state = new SequenceEditorState();
        this.sequence = sequence;
        this.playState = playState;
    }

    public void UpdateEditor() {
        if (Input.GetKeyDown(KeyCode.F1))
            Visible = !Visible;

        if (!Visible)
            return;
        
        bool wasShowingValue = state.ShowValue;

        state.ShowValue = false;
        
        bool anyInput = CheckInputs();

        if (!anyInput && wasShowingValue)
            state.ShowValue = true;

        state.Time = playState.currentTrackTick;
        
        if (!selecting || anyInput || state.Time == state.SelectionEndTime)
            return;
        
        state.SelectionEndTime = state.Time;
        UpdateSelection();
        state.ShowValue = false;
    }

    public void Exit() => sequence = new TrackVisualsEventSequence();

    public List<TrackVisualsEvent> GetSequenceAsVisualsEvents() => sequence.ToVisualsEvents();

    private void MoveTime(int direction, bool largeMovement, bool smallMovement, bool changeSelection, bool moveSelected) {
        if (largeMovement)
            direction *= 8;

        var trackEditor = Track.Instance.trackEditor;

        if (smallMovement || moveSelected) {
            float directionFloat = 0.125f * direction;

            if (smallMovement)
                directionFloat *= 0.125f;
            
            trackEditor.SetCurrentTrackTime(playState.trackData.GetTimeOffsetByTicks(TICK_TO_TIME * state.Time, directionFloat), false);
        }
        else
            trackEditor.SetCurrentTrackTime(trackEditor.GetQuantizedMoveTime(TICK_TO_TIME * state.Time, direction), false);

        long previousTime = state.Time;
        
        state.Time = playState.currentTrackTick;

        if (moveSelected)
            MoveSelectedByTime(state.Time - previousTime);
        else if (changeSelection) {
            state.SelectionEndTime = state.Time;
            UpdateSelection();
        }
        else {
            ClearSelection();
            UpdateSelection();
        }
    }
    
    private void MoveCursorIndex(int direction, bool largeMovement, bool changeSelection, bool moveSelected) {
        if (largeMovement)
            direction *= 8;
        
        state.CursorIndex = Mod(state.CursorIndex + direction, 256);
        state.ColumnPan = Mathf.Clamp(state.ColumnPan, state.CursorIndex - (COLUMN_COUNT - 2), state.CursorIndex - 1);
        state.ColumnPan = Mathf.Clamp(state.ColumnPan, 0, 240);

        if (moveSelected) {
            MoveSelectedByIndex(direction);
        }
        else if (changeSelection) {
            state.SelectionEndIndex = state.CursorIndex;
            UpdateSelection();
        }
        else {
            ClearSelection();
            UpdateSelection();
        }
    }

    private void ChangeValue(int direction, bool largeAmount) {
        if (largeAmount)
            direction *= 16;
        
        var onOffEvents = sequence.OnOffEvents;
        
        foreach (int index in state.SelectedIndicesPerColumn[0]) {
            var onOffEvent = onOffEvents[index];

            onOffEvent.Value = Mathf.Clamp(onOffEvent.Value + direction, 0, 255);
        }

        state.ShowValue = true;
    }

    private void BeginSelection() {
        ClearSelection();
        UpdateSelection();
        selecting = true;
    }

    private void PlaceOnOffEventAtCursor(OnOffEventType type) {
        ClearSelection();
        UpdateSelection();
        DeleteSelected();
        
        if (TimeInBounds(state.Time)) {
            var onOffEvents = sequence.OnOffEvents;
            var onOffEvent = new OnOffEvent(state.Time, type, state.CursorIndex, 255);
            int index = onOffEvents.GetInsertIndex(onOffEvent);
            
            onOffEvents.Insert(index, onOffEvent);

            for (int i = index - 1; i >= 0; i--) {
                var other = onOffEvents[i];

                if (other.Type == OnOffEventType.Off || other.Index != onOffEvent.Index)
                    continue;
                
                onOffEvent.Value = other.Value;

                break;
            }
        }
        
        UpdateSelection();
    }

    private void Delete() {
        DeleteSelected();
        UpdateSelection();
    }

    private void Copy() {
        var selectedIndices = state.SelectedIndicesPerColumn[0];
        
        if (selectedIndices.Count == 0)
            return;
        
        var onOffEvents = sequence.OnOffEvents;
        long firstTime = onOffEvents[selectedIndices[0]].Time;
        int firstIndex = int.MaxValue;

        foreach (int index in selectedIndices) {
            int eventIndex = onOffEvents[index].Index;

            if (eventIndex < firstIndex)
                firstIndex = eventIndex;
        }
        
        onOffEventClipboard.Clear();

        foreach (int index in selectedIndices) {
            var newEvent = new OnOffEvent(onOffEvents[index]);

            newEvent.Time -= firstTime;
            newEvent.Index -= firstIndex;
            onOffEventClipboard.Add(newEvent);
        }
    }

    private void Cut() {
        Copy();
        DeleteSelected();
        UpdateSelection();
    }

    private void Paste() {
        long time = state.Time;
        int cursorIndex = state.CursorIndex;
        var selectedIndices = state.SelectedIndicesPerColumn[0];
        var onOffEvents = sequence.OnOffEvents;
        
        ClearSelection();

        foreach (var onOffEvent in onOffEventClipboard) {
            var newEvent = new OnOffEvent(onOffEvent);

            newEvent.Time += time;
            newEvent.Index = Mod(newEvent.Index + cursorIndex, 256);

            if (!TimeInBounds(newEvent.Time))
                continue;
            
            int index = onOffEvents.GetInsertIndex(newEvent);

            onOffEvents.Insert(index, newEvent);
            selectedIndices.Add(index);
        }
    }

    private void MoveSelectedByTime(long amount) {
        var selectedIndices = state.SelectedIndicesPerColumn[0];
        var onOffEvents = sequence.OnOffEvents;
        var toAdd = new List<OnOffEvent>();

        foreach (int index in selectedIndices)
            toAdd.Add(onOffEvents[index]);

        for (int i = selectedIndices.Count - 1; i >= 0; i--)
            onOffEvents.RemoveAt(selectedIndices[i]);
        
        selectedIndices.Clear();

        foreach (var onOffEvent in toAdd) {
            onOffEvent.Time += amount;
            
            if (!TimeInBounds(onOffEvent.Time))
                continue;

            int index = onOffEvents.GetInsertIndex(onOffEvent);
            
            onOffEvents.Insert(index, onOffEvent);
            selectedIndices.Add(index);
        }
    }

    private void MoveSelectedByIndex(int amount) {
        var onOffEvents = sequence.OnOffEvents;
        
        foreach (int index in state.SelectedIndicesPerColumn[0]) {
            var onOffEvent = onOffEvents[index];

            onOffEvent.Index = Mod(onOffEvent.Index + amount, 256);
        }
    }

    private void DeleteSelected() {
        var selectedIndices = state.SelectedIndicesPerColumn[0];
        var onOffEvents = sequence.OnOffEvents;
        
        for (int i = selectedIndices.Count - 1; i >= 0; i--)
            onOffEvents.RemoveAt(selectedIndices[i]);

        ClearSelection();
    }

    private void UpdateSelection() {
        var onOffEvents = sequence.OnOffEvents;
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
        
        for (int i = 0; i < 256; i++)
            selectedIndicesPerColumn[i].Clear();

        long startTime = state.SelectionStartTime;
        long endTime = state.SelectionEndTime;
        int startIndex = state.SelectionStartIndex;
        int endIndex = state.SelectionEndIndex;
        var selectedIndices = selectedIndicesPerColumn[0];

        if (endTime < startTime)
            (startTime, endTime) = (endTime, startTime);

        startTime -= SELECTION_EPSILON;
        endTime += SELECTION_EPSILON;

        if (endIndex < startIndex)
            (startIndex, endIndex) = (endIndex, startIndex);

        for (int i = 0; i < onOffEvents.Count; i++) {
            var onOffEvent = onOffEvents[i];
            long time = onOffEvent.Time;
            int index = onOffEvent.Index;

            if (time >= startTime && time <= endTime && index >= startIndex && index <= endIndex)
                selectedIndices.Add(i);
        }
    }
    
    private void ClearSelection() {
        state.SelectionStartTime = state.Time;
        state.SelectionEndTime = state.Time;
        state.SelectionStartIndex = state.CursorIndex;
        state.SelectionEndIndex = state.CursorIndex;

        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;

        for (int i = 0; i < 256; i++)
            selectedIndicesPerColumn[i].Clear();

        selecting = false;
    }

    private bool CheckInputs() {
        if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            selecting = false;
        
        int direction = 0;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            direction++;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            direction--;

        if (direction != 0) {
            MoveTime(
                direction,
                Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt),
                Input.GetKey(KeyCode.F),
                selecting,
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            
            return true;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
            direction++;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            direction--;
        
        if (direction != 0) {
            MoveCursorIndex(
                direction,
                Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt),
                selecting,
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            
            return true;
        }
        
        if (Input.GetKeyDown(KeyCode.Equals))
            direction++;

        if (Input.GetKeyDown(KeyCode.Minus))
            direction--;
        
        if (direction != 0) {
            ChangeValue(
                direction,
                Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
            
            return true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            BeginSelection();
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            PlaceOnOffEventAtCursor(OnOffEventType.OnOff);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            PlaceOnOffEventAtCursor(OnOffEventType.On);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            PlaceOnOffEventAtCursor(OnOffEventType.Off);
        else if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            Delete();
        else if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            Copy();
        else if (Input.GetKeyDown(KeyCode.X) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            Cut();
        else if (Input.GetKeyDown(KeyCode.V) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            Paste();
        else
            return false;

        return true;
    }

    private bool TimeInBounds(long time) => time >= 0 && time < TIME_TO_TICK * playState.trackData.SoundEndTime;

    private static int Mod(int a, int b) => (a % b + b) % b;
}