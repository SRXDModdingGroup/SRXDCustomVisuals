using System;
using UnityEngine;
using Input = UnityEngine.Input;

namespace SRXDCustomVisuals.Plugin;

public class SequenceEditor : MonoBehaviour {
    private const float TICK_TO_TIME = 0.00001f;
    private const float WINDOW_WIDTH = 800;
    private const float WINDOW_HEIGHT = 600;
    private const int COLUMN_COUNT = 16;
    private const long SELECTION_EPSILON = 1000L;
    
    public bool Visible { get; set; }

    private SequenceEditorState state;
    private SequenceRenderer renderer;
    private PlayState playState;
    private TrackVisualsEventSequence sequence;

    private void Awake() {
        state = new SequenceEditorState();
        renderer = new SequenceRenderer(WINDOW_WIDTH, WINDOW_HEIGHT, COLUMN_COUNT);
        sequence = new TrackVisualsEventSequence();
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
        
        long previousTime = state.Time;
        bool anyInput = CheckInputs();

        state.Time = playState.currentTrackTick;

        if (anyInput || state.Time == previousTime || !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            return;
        
        state.SelectionEndTime = state.Time;
        UpdateSelection();
    }

    public void Exit() => sequence = new TrackVisualsEventSequence();
    
    private void MoveCursorIndex(int direction, bool largeMovement, bool changeSelection, bool moveSelected) {
        if (largeMovement)
            direction *= 8;
        
        state.CursorIndex = Mod(state.CursorIndex + direction, 256);
        state.ColumnPan = Mathf.Clamp(state.ColumnPan, state.CursorIndex - (COLUMN_COUNT - 2), state.CursorIndex - 1);
        state.ColumnPan = Mathf.Clamp(state.ColumnPan, 0, 240);

        if (moveSelected) {
            
        }
        else if (changeSelection) {
            state.SelectionEndIndex = state.CursorIndex;
            UpdateSelection();
        }
        else
            ClearSelection();
    }

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
        
        state.Time = playState.currentTrackTick;

        if (moveSelected) {
            
        }
        else if (changeSelection) {
            state.SelectionEndTime = state.Time;
            UpdateSelection();
        }
        else
            ClearSelection();
    }

    private void PlaceOnOffEventAtCursor(OnOffEventType type) {
        ClearSelection();
        DeleteSelected();
        sequence.OnOffEvents.InsertSorted(new OnOffEvent(state.Time, type, state.CursorIndex, 255));
        ClearSelection();
    }

    private void DeleteSelected() {
        var selectedIndices = state.SelectedIndices;
        var onOffEvents = sequence.OnOffEvents;
        
        for (int i = selectedIndices.Count - 1; i >= 0; i--)
            onOffEvents.RemoveAt(selectedIndices[i]);

        ClearSelection();
    }
    
    private void ClearSelection() {
        state.SelectionStartTime = state.Time;
        state.SelectionEndTime = state.Time;
        state.SelectionStartIndex = state.CursorIndex;
        state.SelectionEndIndex = state.CursorIndex;
        UpdateSelection();
    }

    private void UpdateSelection() {
        var onOffEvents = sequence.OnOffEvents;
        var selectedIndices = state.SelectedIndices;
        
        selectedIndices.Clear();

        long startTime = state.SelectionStartTime;
        long endTime = state.SelectionEndTime;
        int startIndex = state.SelectionStartIndex;
        int endIndex = state.SelectionEndIndex;

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

    private bool CheckInputs() {
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
                Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
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
                Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            
            return true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            ClearSelection();
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            PlaceOnOffEventAtCursor(OnOffEventType.OnOff);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            PlaceOnOffEventAtCursor(OnOffEventType.On);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            PlaceOnOffEventAtCursor(OnOffEventType.Off);
        else if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            DeleteSelected();
        else
            return false;

        return true;
    }

    private static int Mod(int a, int b) => (a % b + b) % b;
}