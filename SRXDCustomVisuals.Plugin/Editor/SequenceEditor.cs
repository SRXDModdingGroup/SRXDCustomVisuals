using System;
using System.Collections.Generic;
using UnityEngine;
using Input = UnityEngine.Input;

namespace SRXDCustomVisuals.Plugin;

public class SequenceEditor : MonoBehaviour {
    private const float TIME_TO_TICK = 100000f;
    private const float TICK_TO_TIME = 0.00001f;
    private const int WINDOW_WIDTH = 800;
    private const int WINDOW_HEIGHT = 600;
    private const int COLUMN_COUNT = 16;
    private const long TIME_EPSILON = 100L;
    
    public bool Visible { get; set; }

    private SequenceEditorState state;
    private SequenceRenderer renderer;
    private TrackVisualsEventSequence sequence;
    private VisualsBackground background;
    private PlayState playState;
    private List<OnOffEvent>[] onOffEventClipboard;
    private List<ControlKeyframe>[] controlCurveClipboard;

    private void Awake() {
        state = new SequenceEditorState();
        renderer = new SequenceRenderer(WINDOW_WIDTH, WINDOW_HEIGHT, COLUMN_COUNT);
        background = VisualsBackground.Empty;
        sequence = new TrackVisualsEventSequence();
        onOffEventClipboard = new List<OnOffEvent>[Constants.IndexCount];
        controlCurveClipboard = new List<ControlKeyframe>[Constants.IndexCount];

        for (int i = 0; i < controlCurveClipboard.Length; i++) {
            onOffEventClipboard[i] = new List<OnOffEvent>();
            controlCurveClipboard[i] = new List<ControlKeyframe>();
        }
    }

    private void OnGUI() {
        if (Visible)
            renderer.Render(new SequenceRenderInput(sequence, background, playState, state));
    }

    public void Init(TrackVisualsEventSequence sequence, VisualsBackground background, PlayState playState) {
        this.sequence = sequence;
        this.background = background;
        this.playState = playState;
        state = new SequenceEditorState();
        
        state.BackgroundField.Init(sequence.Background);

        for (int i = 0; i < Constants.PaletteSize; i++)
            state.PaletteFields[i].Init(Util.ToHexString(sequence.Palette[i]));
    }

    public void Exit() => sequence = new TrackVisualsEventSequence();

    public void UpdateEditor(out bool anyInput, out bool anyEdit) {
        if (Input.GetKeyDown(KeyCode.F1))
            Visible = !Visible;

        anyInput = false;
        anyEdit = false;

        if (!Visible)
            return;
        
        if (Input.GetKeyDown(KeyCode.Tab)) {
            CycleModes();
            
            return;
        }
        
        sequence.BeginEdit();
        CheckFields();
        
        if (state.Mode == SequenceEditorMode.Details) {
            anyEdit = sequence.EndEdit();
            
            return;
        }
        
        bool wasShowingValue = state.ShowValues;

        state.ShowValues = false;
        anyInput = CheckInputs();
        anyEdit = sequence.EndEdit();

        if (!anyInput && wasShowingValue)
            state.ShowValues = true;

        state.Time = playState.currentTrackTick;
        
        if (!state.Selecting || anyInput || state.Time == state.SelectionEndTime)
            return;
        
        state.SelectionEndTime = state.Time;
        UpdateSelection();
        state.ShowValues = false;
    }

    public CustomVisualsInfo GetCustomVisualsInfo() => sequence.ToCustomVisualsInfo();

    private void CycleModes() {
        state.Mode = (SequenceEditorMode) Util.Mod((int) state.Mode + 1, (int) SequenceEditorMode.Count);
        ClearSelection();
        UpdateSelection();
    }

    private void MoveByTime(int direction, bool largeMovement, bool smallMovement, bool moveToNext, bool changeSelection, bool moveSelected) {
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
            JumpToNext(direction);
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

    private void MoveByColumn(int direction, bool largeMovement, bool changeSelection, bool moveSelected) {
        if (largeMovement)
            direction *= 8;
        
        state.Column = Mathf.Clamp(state.Column + direction, 0, sequence.ColumnCount - 1);
        state.ColumnPan = Mathf.Clamp(state.ColumnPan, state.Column - (COLUMN_COUNT - 2), state.Column - 1);
        state.ColumnPan = Mathf.Clamp(state.ColumnPan, 0, 240);

        if (moveSelected)
            MoveSelectedByColumn(direction);
        else if (changeSelection) {
            state.SelectionEndColumn = state.Column;
            UpdateSelection();
        }
        else {
            ClearSelection();
            UpdateSelection();
        }
    }

    private void ChangeType(int direction) {
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                ReplaceSelectedElementsInPlace(sequence.GetHandleForOnOffEvents, onOffEvent
                    => onOffEvent.WithType((OnOffEventType) Util.Mod((int) onOffEvent.Type + direction, (int) OnOffEventType.Count)));
                break;
            case SequenceEditorMode.ControlCurves:
                ReplaceSelectedElementsInPlace(sequence.GetHandleForControlCurves, keyframe
                    => keyframe.WithType((ControlKeyframeType) Util.Mod((int) keyframe.Type + direction, (int) ControlKeyframeType.Count)));
                break;
        }
    }

    private void ChangeValue(int direction, bool largeAmount, bool smallAmount) {
        if (largeAmount)
            direction *= 64;
        else if (!smallAmount)
            direction *= 8;

        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                ReplaceSelectedElementsInPlace(sequence.GetHandleForOnOffEvents, onOffEvent
                    => onOffEvent.WithValue(Mathf.Clamp(onOffEvent.Value + direction, 0, Constants.MaxEventValue)));
                break;
            case SequenceEditorMode.ControlCurves:
                ReplaceSelectedElementsInPlace(sequence.GetHandleForControlCurves, keyframe
                    => keyframe.WithValue(Mathf.Clamp(keyframe.Value + direction, 0, Constants.MaxEventValue)));
                break;
        }

        state.ShowValues = true;
    }

    private void BeginSelection() {
        ClearSelection();
        UpdateSelection();
        state.Selecting = true;
    }

    private void PlaceOnOffEventAtCursor(OnOffEventType type)
        => PlaceElementAtCursor(sequence.GetHandleForOnOffEvents, new OnOffEvent(state.Time, type, Constants.MaxEventValue));

    private void PlaceControlKeyframeAtCursor(ControlKeyframeType type)
        => PlaceElementAtCursor(sequence.GetHandleForControlCurves, new ControlKeyframe(state.Time, type, Constants.MaxEventValue));

    private void EvenSpace() {
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                DoEvenSpace(sequence.GetHandleForOnOffEvents);
                break;
            case SequenceEditorMode.ControlCurves:
                DoEvenSpace(sequence.GetHandleForControlCurves);
                break;
        }

        void DoEvenSpace<T>(SequenceEditHandle<T> handle) where T : ISequenceElement<T> =>
            ReplaceSelectedElementsOutOfPlacePerColumn(handle, elements => {
                var elementsList = new List<T>(elements);
                int[] groupIndices = new int[elementsList.Count];
                int groupIndex = 0;
                long minTime = elementsList[0].Time;
                long timeDiff = elementsList[elementsList.Count - 1].Time - minTime;
                long groupTime = minTime;

                for (int i = 0; i < elementsList.Count; i++) {
                    var element = elementsList[i];
                        
                    if (element.Time - groupTime > TIME_EPSILON) {
                        groupIndex++;
                        groupTime = element.Time;
                    }

                    groupIndices[i] = groupIndex;
                }
                    
                var toAdd = new List<T>(elementsList.Count);

                for (int i = 0; i < elementsList.Count; i++) {
                    var element = elementsList[i];
                        
                    toAdd.Add(element.WithTime(minTime + timeDiff * groupIndices[i] / groupIndex));
                }

                return toAdd;
            });
    }

    private void Quantize() {
        var trackData = playState.trackData;

        Track.Instance.trackEditor.SetCurrentTrackTime(trackData.GetQuantizedTime(TICK_TO_TIME * state.Time, 0.03125f), false);
        state.Time = playState.currentTrackTick;
        
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                DoQuantize(sequence.GetHandleForOnOffEvents);
                break;
            case SequenceEditorMode.ControlCurves:
                DoQuantize(sequence.GetHandleForControlCurves);
                break;
        }

        void DoQuantize<T>(SequenceEditHandle<T> handle) where T : ISequenceElement<T> {
            ReplaceSelectedElementsOutOfPlace(handle, element
                => element.WithTime((long) (TIME_TO_TICK * trackData.GetQuantizedTime(TICK_TO_TIME * element.Time, 0.03125f))));
        }
    }

    private void Delete() {
        DeleteSelected();
        UpdateSelection();
    }

    private void Copy() {
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                DoCopy(sequence.GetHandleForOnOffEvents, onOffEventClipboard);
                break;
            case SequenceEditorMode.ControlCurves:
                DoCopy(sequence.GetHandleForControlCurves, controlCurveClipboard);
                break;
        }

        void DoCopy<T>(SequenceEditHandle<T> handle, List<T>[] clipboard) where T : ISequenceElement<T> {
            long firstTime = long.MaxValue;
            int firstColumn = -1;

            foreach (var column in clipboard)
                column.Clear();

            var collection = handle.Collection;

            for (int i = 0; i < collection.ColumnCount; i++) {
                bool any = false;
                
                foreach (var element in GetSelectedElementsInColumn(collection, i)) {
                    any = true;

                    if (element.Time < firstTime)
                        firstTime = element.Time;
                }

                if (any && firstColumn < 0)
                    firstColumn = i;
            }

            if (firstColumn < 0)
                return;

            for (int i = 0; i < collection.ColumnCount; i++) {
                var clipboardColumn = clipboard[i - firstColumn];
                    
                foreach (var element in GetSelectedElementsInColumn(collection, i))
                    clipboardColumn.Add(element.WithTime(element.Time - firstTime));
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
        int cursorIndex = state.Column;
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
        
        ClearSelection();

        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                DoPaste(sequence.GetHandleForOnOffEvents, onOffEventClipboard);
                break;
            case SequenceEditorMode.ControlCurves:
                DoPaste(sequence.GetHandleForControlCurves, controlCurveClipboard);
                break;
        }
        
        MatchSelectionBoxToSelection();

        void DoPaste<T>(SequenceEditHandle<T> handle, List<T>[] clipboard) where T : ISequenceElement<T> {
            for (int i = 0; i < clipboard.Length; i++) {
                int newColumn = i + cursorIndex;
                
                if (newColumn >= sequence.ColumnCount)
                    break;

                handle.AddElements(newColumn, GetNewElements(i), selectedIndicesPerColumn[newColumn]);

                IEnumerable<T> GetNewElements(int column) {
                    foreach (var element in clipboard[column]) {
                        long newTime = element.Time + time;

                        if (TimeInBounds(newTime))
                            yield return element.WithTime(newTime);
                    }
                }
            }
        }
    }
    
    private void SelectAll(bool all, bool inRow) {
        if (all || inRow) {
            state.SelectionStartColumn = 0;
            state.SelectionEndColumn = sequence.ColumnCount - 1;
        }
        
        if (all || !inRow) {
            state.SelectionStartTime = 0;
            state.SelectionEndTime = (long) (TIME_TO_TICK * playState.trackData.SoundEndTime);
        }
        
        UpdateSelection();
    }

    private void JumpToNext(int direction) {
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                DoJump(sequence.OnOffEvents);
                break;
            case SequenceEditorMode.ControlCurves:
                DoJump(sequence.ControlCurves);
                break;
        }
        
        void DoJump<T>(IReadOnlySequenceElementCollection<T> collection) where T : ISequenceElement<T> {
            var elements = collection.GetElementsInColumn(state.Column);
            
            if (direction > 0) {
                foreach (var element in elements) {
                    if (element.Time <= state.Time + TIME_EPSILON)
                        continue;

                    state.Time = element.Time;

                    break;
                }
            }
            else if (direction < 0) {
                for (int i = elements.Count - 1; i >= 0; i--) {
                    var element = elements[i];
                            
                    if (element.Time >= state.Time - TIME_EPSILON)
                        continue;

                    state.Time = element.Time;

                    break;
                }
            }
        }
    }

    private void MoveSelectedByTime(long amount) {
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                ReplaceSelectedElementsOutOfPlace(sequence.GetHandleForOnOffEvents,
                    onOffEvent => onOffEvent.WithTime(onOffEvent.Time + amount));
                break;
            case SequenceEditorMode.ControlCurves:
                ReplaceSelectedElementsOutOfPlace(sequence.GetHandleForControlCurves,
                    keyframe => keyframe.WithTime(keyframe.Time + amount));
                break;
        }
        
        MatchSelectionBoxToSelection();
    }

    private void MoveSelectedByColumn(int amount) {
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                DoMove(sequence.GetHandleForOnOffEvents);
                break;
            case SequenceEditorMode.ControlCurves:
                DoMove(sequence.GetHandleForControlCurves);
                break;
        }
        
        MatchSelectionBoxToSelection();

        void DoMove<T>(SequenceEditHandle<T> handle) where T : ISequenceElement<T> {
            var collection = handle.Collection;
            var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
            int leftmost = collection.ColumnCount - 1;
            int rightmost = 0;
        
            for (int i = 0; i < collection.ColumnCount; i++) {
                if (selectedIndicesPerColumn[i].Count == 0)
                    continue;

                if (i < leftmost)
                    leftmost = i;

                if (i > rightmost)
                    rightmost = i;
            }

            amount = Mathf.Clamp(amount, -leftmost, sequence.ColumnCount - 1 - rightmost);
        
            if (amount == 0) {
                MatchSelectionBoxToSelection();
            
                return;
            }

            if (amount > 0) {
                for (int i = rightmost; i >= leftmost; i--)
                    MoveColumn(i);
            }
            else {
                for (int i = leftmost; i <= rightmost; i++)
                    MoveColumn(i);
            }
            
            void MoveColumn(int column) {
                int targetColumn = column + amount;
                var selectedIndices = selectedIndicesPerColumn[column];
            
                handle.AddElements(targetColumn, GetSelectedElementsInColumn(collection, column), selectedIndicesPerColumn[targetColumn]);
                handle.RemoveElements(column, selectedIndices);
                selectedIndices.Clear();
            }
        }
    }

    private void PlaceElementAtCursor<T>(SequenceEditHandle<T> handle, T element) where T : ISequenceElement<T> {
        ClearSelection();
        UpdateSelection();
        DeleteSelected();

        if (!TimeInBounds(state.Time)) {
            UpdateSelection();

            return;
        }

        var collection = handle.Collection;
        int index = collection.GetFirstIndexAfterTime(state.Column, element.Time);

        if (index > 0)
            element = element.WithValue(collection.GetElement(state.Column, index - 1).Value);

        handle.InsertElement(state.Column, index, element);
        UpdateSelection();
    }

    private void DeleteSelected() {
        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                DoDelete(sequence.GetHandleForOnOffEvents);
                break;
            case SequenceEditorMode.ControlCurves:
                DoDelete(sequence.GetHandleForControlCurves);
                break;
        }

        ClearSelection();

        void DoDelete<T>(SequenceEditHandle<T> handle) where T : ISequenceElement<T> {
            var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
            
            for (int i = 0; i < sequence.ColumnCount; i++)
                handle.RemoveElements(i, selectedIndicesPerColumn[i]);
        }
    }

    private void UpdateSelection() {
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
        long startTime = state.SelectionStartTime;
        long endTime = state.SelectionEndTime;
        int startColumn = state.SelectionStartColumn;
        int endColumn = state.SelectionEndColumn;
        
        if (endTime < startTime)
            (startTime, endTime) = (endTime, startTime);

        startTime -= TIME_EPSILON;
        endTime += TIME_EPSILON;

        if (endColumn < startColumn)
            (startColumn, endColumn) = (endColumn, startColumn);

        foreach (var selectedIndices in selectedIndicesPerColumn)
            selectedIndices.Clear();

        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                DoSelect(sequence.GetHandleForOnOffEvents);
                break;
            case SequenceEditorMode.ControlCurves:
                DoSelect(sequence.GetHandleForControlCurves);
                break;
        }

        void DoSelect<T>(SequenceEditHandle<T> handle) where T : ISequenceElement<T> {
            var collection = handle.Collection;
            
            for (int i = startColumn; i <= endColumn; i++) {
                var elements = collection.GetElementsInColumn(i);
                var selectedIndices = selectedIndicesPerColumn[i];
                
                for (int j = 0; j < elements.Count; j++) {
                    var element = elements[j];
                    
                    if (element.Time > endTime)
                        break;

                    if (element.Time >= startTime)
                        selectedIndices.Add(j);
                }
            }
        }
    }

    private void ClearSelection() {
        state.SelectionStartTime = state.Time;
        state.SelectionEndTime = state.Time;
        state.SelectionStartColumn = state.Column;
        state.SelectionEndColumn = state.Column;

        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;

        foreach (var selectedIndices in selectedIndicesPerColumn)
            selectedIndices.Clear();

        state.Selecting = false;
    }

    private void MatchSelectionBoxToSelection() {
        state.SelectionStartTime = long.MaxValue;
        state.SelectionEndTime = -1;
        state.SelectionStartColumn = int.MaxValue;
        state.SelectionEndColumn = -1;
        
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;

        switch (state.Mode) {
            case SequenceEditorMode.OnOffEvents:
                DoMatchToSelection(sequence.GetHandleForOnOffEvents);
                break;
            case SequenceEditorMode.ControlCurves:
                DoMatchToSelection(sequence.GetHandleForControlCurves);
                break;
        }

        if (state.SelectionEndTime >= 0)
            return;
        
        ClearSelection();
        UpdateSelection();

        void DoMatchToSelection<T>(SequenceEditHandle<T> handle) where T : ISequenceElement<T> {
            var collection = handle.Collection;
            
            for (int i = 0; i < selectedIndicesPerColumn.Length; i++) {
                if (selectedIndicesPerColumn[i].Count == 0)
                    continue;

                foreach (var element in GetSelectedElementsInColumn(collection, i)) {
                    if (element.Time < state.SelectionStartTime)
                        state.SelectionStartTime = element.Time;

                    if (element.Time > state.SelectionEndTime)
                        state.SelectionEndTime = element.Time;
                }
                    
                if (i < state.SelectionStartColumn)
                    state.SelectionStartColumn = i;

                if (i > state.SelectionEndColumn)
                    state.SelectionEndColumn = i;
            }
        }
    }

    private void CheckFields() {
        if (state.BackgroundField.CheckValueChanged()) {
            string value = state.BackgroundField.DisplayValue.Trim();

            sequence.Background = value;
            state.BackgroundField.ActualValue = value;
        }

        var paletteFields = state.PaletteFields;
        
        for (int i = 0; i < paletteFields.Count; i++) {
            var field = paletteFields[i];
            
            if (!field.CheckValueChanged() || !Util.TryParseColor32(field.DisplayValue, out var color))
                continue;
            
            sequence.SetPaletteColor(i, color);
            field.ActualValue = Util.ToHexString(color);
        }
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
            MoveByTime(direction,
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
            MoveByColumn(direction,
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
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                ChangeType(direction);
            else {
                ChangeValue(direction,
                    Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt),
                    Input.GetKey(KeyCode.F));
            }
            
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
                    PlaceOnOffEventAtCursor(OnOffEventType.Off);
                    break;
                case SequenceEditorMode.ControlCurves:
                    PlaceControlKeyframeAtCursor(ControlKeyframeType.Linear);
                    break;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && state.Mode == SequenceEditorMode.ControlCurves)
            PlaceControlKeyframeAtCursor(ControlKeyframeType.EaseOut);
        else if (Input.GetKeyDown(KeyCode.Alpha5) && state.Mode == SequenceEditorMode.ControlCurves)
            PlaceControlKeyframeAtCursor(ControlKeyframeType.EaseIn);
        else if (Input.GetKeyDown(KeyCode.E))
            EvenSpace();
        else if (Input.GetKeyDown(KeyCode.Q))
            Quantize();
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
        else if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            sequence.Undo();
        else if (Input.GetKeyDown(KeyCode.Y) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            sequence.Redo();
        else
            return false;

        return true;
    }

    private bool TimeInBounds(long time) => time >= 0 && time < TIME_TO_TICK * playState.trackData.SoundEndTime;

    private void ReplaceSelectedElementsInPlace<T>(SequenceEditHandle<T> handle, Func<T, T> func) where T : ISequenceElement<T> {
        var collection = handle.Collection;
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
        
        for (int i = 0; i < collection.ColumnCount; i++) {
            var elements = collection.GetElementsInColumn(i);

            foreach (int index in selectedIndicesPerColumn[i])
                handle.ReplaceElement(i, index, func(elements[index]));
        }
    }

    private void ReplaceSelectedElementsOutOfPlace<T>(SequenceEditHandle<T> handle, Func<T, T> func) where T : ISequenceElement<T> {
        var collection = handle.Collection;
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
        var toAdd = new List<T>();
        
        for (int i = 0; i < collection.ColumnCount; i++) {
            foreach (var element in GetSelectedElementsInColumn(collection, i)) {
                var newElement = func(element);
                
                if (TimeInBounds(newElement.Time))
                    toAdd.Add(newElement);
            }
            
            var selectedIndices = selectedIndicesPerColumn[i];
            
            handle.RemoveElements(i, selectedIndices);
            handle.AddElements(i, toAdd, selectedIndices);
            toAdd.Clear();
        }
    }

    private void ReplaceSelectedElementsOutOfPlacePerColumn<T>(SequenceEditHandle<T> handle, Func<IEnumerable<T>, IEnumerable<T>> func) where T : ISequenceElement<T> {
        var collection = handle.Collection;
        var selectedIndicesPerColumn = state.SelectedIndicesPerColumn;
        var toAdd = new List<T>();
        
        for (int i = 0; i < collection.ColumnCount; i++) {
            var selectedIndices = selectedIndicesPerColumn[i];
            
            foreach (var element in func(GetSelectedElementsInColumn(collection, i))) {
                if (TimeInBounds(element.Time))
                    toAdd.Add(element);
            }

            handle.RemoveElements(i, selectedIndices);
            handle.AddElements(i, toAdd, selectedIndices);
            toAdd.Clear();
        }
    }

    private IEnumerable<T> GetSelectedElementsInColumn<T>(IReadOnlySequenceElementCollection<T> collection, int column) where T : ISequenceElement<T> {
        var elements = collection.GetElementsInColumn(column);

        foreach (int index in state.SelectedIndicesPerColumn[column])
            yield return elements[index];
    }
}