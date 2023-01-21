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
    private const int INDEX_COUNT = 256;
    private const int COLUMN_COUNT = 16;
    private const int VALUE_MAX = 255;
    private const long SELECTION_EPSILON = 100L;
    
    public bool Visible { get; set; }

    public bool Dirty => true;

    private SequenceEditorState state;
    private SequenceRenderer renderer;
    private PlayState playState;
    private TrackVisualsEventSequence sequence;
    private List<OnOffEvent> onOffEventClipboard;
    private List<ControlKeyframe>[] controlKeyframeClipboard;

    private void Awake() {
        state = new SequenceEditorState();
        renderer = new SequenceRenderer(WINDOW_WIDTH, WINDOW_HEIGHT, COLUMN_COUNT);
        sequence = new TrackVisualsEventSequence();
        onOffEventClipboard = new List<OnOffEvent>();
        controlKeyframeClipboard = new List<ControlKeyframe>[INDEX_COUNT];

        for (int i = 0; i < INDEX_COUNT; i++)
            controlKeyframeClipboard[i] = new List<ControlKeyframe>();
    }

    private void OnGUI() {
        if (Visible)
            renderer.Render(new RenderInfo(playState, state, sequence));
    }

    public void Init(TrackVisualsEventSequence sequence, PlayState playState) {
        this.sequence = sequence;
        this.playState = playState;
        state = new SequenceEditorState {
            BackgroundField = sequence.Background
        };
    }

    public void UpdateEditor() {
        if (Input.GetKeyDown(KeyCode.F1))
            Visible = !Visible;

        if (!Visible)
            return;
        
        if (Input.GetKeyDown(KeyCode.Home)) {
            CycleModes();
            
            return;
        }
        
        if (state.Mode == SequenceEditorMode.Details) {
            sequence.Background = state.BackgroundField;
            
            return;
        }
        
        bool wasShowingValue = state.ShowValues;

        state.ShowValues = false;
        
        bool anyInput = CheckInputs();

        if (!anyInput && wasShowingValue)
            state.ShowValues = true;

        state.Time = playState.currentTrackTick;
        
        if (!state.Selecting || anyInput || state.Time == state.SelectionEndTime)
            return;
        
        state.SelectionEndTime = state.Time;
        UpdateSelection();
        state.ShowValues = false;
    }

    public void Exit() => sequence = new TrackVisualsEventSequence();

    public CustomVisualsInfo GetCustomVisualsInfo() => sequence.ToCustomVisualsInfo();

    private void MoveTime(int direction, bool largeMovement, bool smallMovement, bool moveToNext, bool changeSelection, bool moveSelected) {
        if (largeMovement)
            direction *= 8;

        var trackEditor = Track.Instance.trackEditor;

        if (smallMovement || moveSelected) {
            float directionFloat = 0.125f * direction;

            if (smallMovement)
                directionFloat *= 0.125f;
            
            trackEditor.SetCurrentTrackTime(playState.trackData.GetTimeOffsetByTicks(TICK_TO_TIME * state.Time, directionFloat), false);
        }
        else if (moveToNext) {
            long time = state.Time;
            int cursorIndex = state.CursorIndex;
            
            switch (state.Mode) {
                case SequenceEditorMode.OnOffEvents: {
                    var onOffEvents = sequence.OnOffEvents;

                    if (direction > 0) {
                        foreach (var onOffEvent in onOffEvents) {
                            if (onOffEvent.Index != cursorIndex || onOffEvent.Time <= time + SELECTION_EPSILON)
                                continue;

                            state.Time = onOffEvent.Time;

                            break;
                        }
                    }
                    else if (direction < 0) {
                        for (int i = onOffEvents.Count - 1; i >= 0; i--) {
                            var onOffEvent = onOffEvents[i];
                            
                            if (onOffEvent.Index != cursorIndex || onOffEvent.Time >= time - SELECTION_EPSILON)
                                continue;

                            state.Time = onOffEvent.Time;

                            break;
                        }
                    }

                    break;
                }
                case SequenceEditorMode.ControlCurves: {
                    var keyframes = sequence.ControlCurves[state.CursorIndex].Keyframes;

                    if (direction > 0) {
                        foreach (var keyframe in keyframes) {
                            if (keyframe.Time <= time + SELECTION_EPSILON)
                                continue;

                            state.Time = keyframe.Time;

                            break;
                        }
                    }
                    else if (direction < 0) {
                        for (int i = keyframes.Count - 1; i >= 0; i--) {
                            var keyframe = keyframes[i];
                            
                            if (keyframe.Time >= time - SELECTION_EPSILON)
                                continue;

                            state.Time = keyframe.Time;

                            break;
                        }
                    }

                    break;
                }
            }
            
            trackEditor.SetCurrentTrackTime(TICK_TO_TIME * state.Time, false);
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
        
        state.CursorIndex = Mod(state.CursorIndex + direction, INDEX_COUNT);
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
        
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;

        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents: {
                var onOffEvents = sequence.OnOffEvents;
        
                foreach (int index in selectedIndicesPerColumn[0]) {
                    var onOffEvent = onOffEvents[index];

                    onOffEvent.Value = Mathf.Clamp(onOffEvent.Value + direction, 0, VALUE_MAX);
                }
                
                break;
            }
            case SequenceEditorMode.ControlCurves: {
                var controlCurves = sequence.ControlCurves;

                for (int i = 0; i < INDEX_COUNT; i++) {
                    var keyframes = controlCurves[i].Keyframes;

                    foreach (int index in selectedIndicesPerColumn[i]) {
                        var keyframe = keyframes[index];

                        keyframe.Value = Mathf.Clamp(keyframe.Value + direction, 0, VALUE_MAX);
                    }
                }
                
                break;
            }
        }

        state.ShowValues = true;
    }

    private void CycleModes() {
        state.Mode = (SequenceEditorMode) Mod((int) state.Mode + 1, (int) SequenceEditorMode.Count);
        ClearSelection();
        UpdateSelection();
    }

    private void BeginSelection() {
        ClearSelection();
        UpdateSelection();
        state.Selecting = true;
    }

    private void PlaceOnOffEventAtCursor(OnOffEventType type) {
        ClearSelection();
        UpdateSelection();
        DeleteSelected();
        
        if (TimeInBounds(state.Time)) {
            var onOffEvents = sequence.OnOffEvents;
            var onOffEvent = new OnOffEvent(state.Time, type, state.CursorIndex, VALUE_MAX);
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

    private void PlaceControlKeyframeAtCursor(ControlKeyframeType type) {
        ClearSelection();
        UpdateSelection();
        DeleteSelected();
        
        if (TimeInBounds(state.Time)) {
            var controlKeyframes = sequence.ControlCurves[state.CursorIndex].Keyframes;
            var keyframe = new ControlKeyframe(state.Time, type, 0);
            int index = controlKeyframes.GetInsertIndex(keyframe);
            
            controlKeyframes.Insert(index, keyframe);

            if (index > 0)
                keyframe.Value = controlKeyframes[index - 1].Value;
        }
        
        UpdateSelection();
    }

    private void Delete() {
        DeleteSelected();
        UpdateSelection();
    }

    private void Copy() {
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;

        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents: {
                var selectedIndices = selectedIndicesPerColumn[0];
        
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
                
                break;
            }
            case SequenceEditorMode.ControlCurves: {
                var controlCurves = sequence.ControlCurves;
                long firstTime = long.MaxValue;
                int firstColumn = -1;
                int lastColumn = 0;

                for (int i = 0; i < INDEX_COUNT; i++) {
                    controlKeyframeClipboard[i].Clear();

                    var selectedIndices = selectedIndicesPerColumn[i];
                    
                    if (selectedIndices.Count == 0)
                        continue;

                    if (firstColumn < 0)
                        firstColumn = i;

                    lastColumn = i;

                    long time = controlCurves[i].Keyframes[selectedIndices[0]].Time;

                    if (time < firstTime)
                        firstTime = time;
                }

                if (firstColumn < 0)
                    break;

                for (int i = firstColumn, j = 0; i <= lastColumn; i++, j++) {
                    var clipboardColumn = controlKeyframeClipboard[j];
                    var keyframes = controlCurves[i].Keyframes;

                    foreach (int index in selectedIndicesPerColumn[i]) {
                        var newKeyframe = new ControlKeyframe(keyframes[index]);

                        newKeyframe.Time -= firstTime;
                        clipboardColumn.Add(newKeyframe);
                    }
                }
                
                break;
            }
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
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
        
        ClearSelection();

        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents: {
                var selectedIndices = selectedIndicesPerColumn[0];
                var onOffEvents = sequence.OnOffEvents;

                foreach (var onOffEvent in onOffEventClipboard) {
                    var newEvent = new OnOffEvent(onOffEvent);

                    newEvent.Time += time;
                    newEvent.Index = Mod(newEvent.Index + cursorIndex, INDEX_COUNT);

                    if (!TimeInBounds(newEvent.Time))
                        continue;
            
                    int index = onOffEvents.GetInsertIndex(newEvent);

                    onOffEvents.Insert(index, newEvent);
                    selectedIndices.Add(index);
                }
                
                break;
            }
            case SequenceEditorMode.ControlCurves: {
                var controlCurves = sequence.ControlCurves;

                for (int i = 0; i < INDEX_COUNT; i++) {
                    int newColumn = Mod(i + cursorIndex, INDEX_COUNT);
                    var keyframes = controlCurves[newColumn].Keyframes;
                    var selectedIndices = selectedIndicesPerColumn[newColumn];

                    foreach (var keyframe in controlKeyframeClipboard[i]) {
                        var newKeyframe = new ControlKeyframe(keyframe);

                        newKeyframe.Time += time;
                        
                        if (!TimeInBounds(newKeyframe.Time))
                            continue;

                        int index = keyframes.GetInsertIndex(newKeyframe);
                        
                        keyframes.Insert(index, newKeyframe);
                        selectedIndices.Add(index);
                    }
                    
                }
                
                break;
            }
        }
        
        MatchSelectionBoxToSelection();
    }
    
    private void SelectAll(bool all, bool inRow) {
        if (all || inRow) {
            state.SelectionStartIndex = 0;
            state.SelectionEndIndex = INDEX_COUNT - 1;
        }
        
        if (all || !inRow) {
            state.SelectionStartTime = 0;
            state.SelectionEndTime = (long) (TIME_TO_TICK * playState.trackData.SoundEndTime);
        }
        
        UpdateSelection();
    }
    
    private void MoveSelectedByTime(long amount) {
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents: {
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
                
                break;
            }
            case SequenceEditorMode.ControlCurves: {
                var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
                var controlCurves = sequence.ControlCurves;
                var toAdd = new List<ControlKeyframe>();

                for (int i = 0; i < INDEX_COUNT; i++) {
                    var selectedIndices = selectedIndicesPerColumn[i];
                    var keyframes = controlCurves[i].Keyframes;
                    
                    toAdd.Clear();

                    foreach (int index in selectedIndices)
                        toAdd.Add(keyframes[index]);

                    for (int j = selectedIndices.Count - 1; j >= 0; j--)
                        keyframes.RemoveAt(selectedIndices[j]);
                    
                    selectedIndices.Clear();

                    foreach (var keyframe in toAdd) {
                        keyframe.Time += amount;
                        
                        if (!TimeInBounds(keyframe.Time))
                            continue;

                        int index = keyframes.GetInsertIndex(keyframe);
            
                        keyframes.Insert(index, keyframe);
                        selectedIndices.Add(index);
                    }
                }
                
                break;
            }
        }
        
        MatchSelectionBoxToSelection();
    }

    private void MoveSelectedByIndex(int amount) {
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
        
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents: {
                var selectedIndices = selectedIndicesPerColumn[0];
                var onOffEvents = sequence.OnOffEvents;
                var toAdd = new List<OnOffEvent>();

                foreach (int index in selectedIndices)
                    toAdd.Add(onOffEvents[index]);

                for (int i = selectedIndices.Count - 1; i >= 0; i--)
                    onOffEvents.RemoveAt(selectedIndices[i]);
        
                selectedIndices.Clear();

                foreach (var onOffEvent in toAdd) {
                    onOffEvent.Index = Mod(onOffEvent.Index + amount, INDEX_COUNT);

                    int index = onOffEvents.GetInsertIndex(onOffEvent);
            
                    onOffEvents.Insert(index, onOffEvent);
                    selectedIndices.Add(index);
                }

                break;
            }
            case SequenceEditorMode.ControlCurves: {
                var controlCurves = sequence.ControlCurves;
                var toAdd = new List<ControlKeyframe>();
                var newColumns = new List<int>();

                for (int i = 0; i < INDEX_COUNT; i++) {
                    var selectedIndices = selectedIndicesPerColumn[i];
                    var keyframes = controlCurves[i].Keyframes;
                    int newColumn = Mod(i + amount, INDEX_COUNT);

                    foreach (int index in selectedIndices) {
                        toAdd.Add(keyframes[index]);
                        newColumns.Add(newColumn);
                    }

                    for (int j = selectedIndices.Count - 1; j >= 0; j--)
                        keyframes.RemoveAt(selectedIndices[j]);
                    
                    selectedIndices.Clear();
                }

                for (int i = 0; i < toAdd.Count; i++) {
                    var keyframe = toAdd[i];
                    int newColumn = newColumns[i];
                    var keyframes = controlCurves[newColumn].Keyframes;
                    int index = keyframes.GetInsertIndex(keyframe);

                    keyframes.Insert(index, keyframe);
                    selectedIndicesPerColumn[newColumn].Add(index);
                }

                break;
            }
        }
        
        MatchSelectionBoxToSelection();
    }

    private void DeleteSelected() {
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents: {
                var selectedIndices = state.SelectedIndicesPerColumn[0];
                var onOffEvents = sequence.OnOffEvents;

                for (int i = selectedIndices.Count - 1; i >= 0; i--)
                    onOffEvents.RemoveAt(selectedIndices[i]);
                
                break;
            }
            case SequenceEditorMode.ControlCurves: {
                var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
                var controlCurves = sequence.ControlCurves;

                for (int i = 0; i < INDEX_COUNT; i++) {
                    var selectedIndices = selectedIndicesPerColumn[i];
                    var keyframes = controlCurves[i].Keyframes;

                    for (int j = selectedIndices.Count - 1; j >= 0; j--)
                        keyframes.RemoveAt(selectedIndices[j]);
                }
                
                break;
            }
        }

        ClearSelection();
    }

    private void UpdateSelection() {
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
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

        for (int i = 0; i < INDEX_COUNT; i++)
            selectedIndicesPerColumn[i].Clear();
        
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents: {
                var onOffEvents = sequence.OnOffEvents;
                var selectedIndices = selectedIndicesPerColumn[0];

                for (int i = 0; i < onOffEvents.Count; i++) {
                    var onOffEvent = onOffEvents[i];
                    long time = onOffEvent.Time;

                    if (time > endTime)
                        break;
                
                    int index = onOffEvent.Index;

                    if (time >= startTime && index >= startIndex && index <= endIndex)
                        selectedIndices.Add(i);
                }

                break;
            }
            case SequenceEditorMode.ControlCurves: {
                var controlCurves = sequence.ControlCurves;

                for (int i = startIndex; i <= endIndex; i++) {
                    var keyframes = controlCurves[i].Keyframes;
                    var selectedIndices = selectedIndicesPerColumn[i];

                    for (int j = 0; j < keyframes.Count; j++) {
                        var keyframe = keyframes[j];
                        long time = keyframe.Time;
                    
                        if (time > endTime)
                            break;
                    
                        if (time >= startTime)
                            selectedIndices.Add(j);
                    }
                }

                break;
            }
        }
    }

    private void ClearSelection() {
        state.SelectionStartTime = state.Time;
        state.SelectionEndTime = state.Time;
        state.SelectionStartIndex = state.CursorIndex;
        state.SelectionEndIndex = state.CursorIndex;

        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;

        for (int i = 0; i < INDEX_COUNT; i++)
            selectedIndicesPerColumn[i].Clear();

        state.Selecting = false;
    }

    private void MatchSelectionBoxToSelection() {
        state.SelectionStartTime = long.MaxValue;
        state.SelectionEndTime = -1;
        state.SelectionStartIndex = int.MaxValue;
        state.SelectionEndIndex = -1;
        
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;

        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents: {
                var onOffEvents = sequence.OnOffEvents;
        
                foreach (int eventIndex in selectedIndicesPerColumn[0]) {
                    var onOffEvent = onOffEvents[eventIndex];
                    long time = onOffEvent.Time;
                    int index = onOffEvent.Index;
                    
                    if (time < state.SelectionStartTime)
                        state.SelectionStartTime = time;

                    if (time > state.SelectionEndTime)
                        state.SelectionEndTime = time;
                    
                    if (index < state.SelectionStartIndex)
                        state.SelectionStartIndex = index;

                    if (index > state.SelectionEndIndex)
                        state.SelectionEndIndex = index;
                }
                
                break;
            }
            case SequenceEditorMode.ControlCurves: {
                var controlCurves = sequence.ControlCurves;

                for (int i = 0; i < INDEX_COUNT; i++) {
                    var selectedIndices = selectedIndicesPerColumn[i];
                    
                    if (selectedIndices.Count == 0)
                        continue;

                    var keyframes = controlCurves[i].Keyframes;
                    
                    foreach (int index in selectedIndices) {
                        long time = keyframes[index].Time;
                        
                        if (time < state.SelectionStartTime)
                            state.SelectionStartTime = time;

                        if (time > state.SelectionEndTime)
                            state.SelectionEndTime = time;
                    }
                    
                    if (i < state.SelectionStartIndex)
                        state.SelectionStartIndex = i;

                    if (i > state.SelectionEndIndex)
                        state.SelectionEndIndex = i;
                }
                
                break;
            }
        }

        if (state.SelectionEndTime >= 0)
            return;
        
        ClearSelection();
        UpdateSelection();
    }

    private bool CheckInputs() {
        if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            state.Selecting = false;
        
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
                Input.GetKey(KeyCode.D),
                state.Selecting,
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
                state.Selecting,
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
        else if (Input.GetKeyDown(KeyCode.Alpha1)) {
            switch (state.Mode) {
                case SequenceEditorMode.OnOffEvents:
                    PlaceOnOffEventAtCursor(OnOffEventType.OnOff);
                    break;
                case SequenceEditorMode.ControlCurves:
                    PlaceControlKeyframeAtCursor(ControlKeyframeType.Constant);
                    break;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            switch (state.Mode) {
                case SequenceEditorMode.OnOffEvents:
                    PlaceOnOffEventAtCursor(OnOffEventType.On);
                    break;
                case SequenceEditorMode.ControlCurves:
                    PlaceControlKeyframeAtCursor(ControlKeyframeType.Smooth);
                    break;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            switch (state.Mode) {
                case SequenceEditorMode.OnOffEvents:
                    PlaceOnOffEventAtCursor(OnOffEventType.OnOff);
                    break;
                case SequenceEditorMode.ControlCurves:
                    PlaceControlKeyframeAtCursor(ControlKeyframeType.Linear);
                    break;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && state.Mode == SequenceEditorMode.ControlCurves)
            PlaceControlKeyframeAtCursor(ControlKeyframeType.EaseIn);
        else if (Input.GetKeyDown(KeyCode.Alpha5) && state.Mode == SequenceEditorMode.ControlCurves)
            PlaceControlKeyframeAtCursor(ControlKeyframeType.EaseOut);
        else if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            Delete();
        else if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            Copy();
        else if (Input.GetKeyDown(KeyCode.X) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            Cut();
        else if (Input.GetKeyDown(KeyCode.V) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            Paste();
        else if (Input.GetKeyDown(KeyCode.A)) {
            SelectAll(
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl),
                Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
        }
        else
            return false;

        return true;
    }

    private bool TimeInBounds(long time) => time >= 0 && time < TIME_TO_TICK * playState.trackData.SoundEndTime;

    private static int Mod(int a, int b) => (a % b + b) % b;
}