using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin;

public class SequenceRenderer {
    private const float TIME_TO_TICK = 100000f;
    private const float TICK_TO_TIME = 0.00001f;
    private const float BOTTOM_TIME_OFFSET = -0.25f;
    private const float TOP_TIME_OFFSET = 1f;
    private const float SIDE_PADDING = 8f;
    private const float TOP_PADDING = 20f;
    private const float ON_EVENT_HEIGHT = 6f;
    private const float OFF_EVENT_HEIGHT = 2f;
    private const float OFF_EVENT_HEIGHT_SELECTED = 4f;
    private const float ON_OFF_EVENT_PADDING = 4f;
    private const float CONTROL_KEYFRAME_SIZE = 5f;
    private const float CONTROL_KEYFRAME_SIZE_SELECTED = 9f;
    private const float CONTROL_CURVE_PADDING = 4f;
    private const float VALUE_BAR_HEIGHT = 2f;
    private const float INFO_LABEL_HEIGHT = 20f;
    private const float VALUE_LABEL_HEIGHT = 20f;
    private const float FIELD_HEIGHT = 20f;
    private const float FIELD_LABEL_WIDTH = 100f;
    private const float DETAILS_START_Y = 60f;
    private static readonly Color BACKING_COLOR = new(0f, 0f, 0f, 0.75f);
    private static readonly Color BEAT_BAR_COLOR = new(0.5f, 0.5f, 0.5f);
    private static readonly Color NOW_BAR_COLOR = Color.white;
    private static readonly Color COLUMN_BOX_COLOR = new(0.5f, 0.5f, 0.5f, 0.05f);
    private static readonly Color COLUMN_BOX_COLOR_SELECTED = new(0.5f, 1f, 1f, 0.1f);
    private static readonly Color ON_EVENT_COLOR = new(0f, 0.4f, 0.8f);
    private static readonly Color OFF_EVENT_COLOR = Color.white;
    private static readonly Color ON_EVENT_COLOR_SELECTED = new(0.75f, 0.875f, 1f);
    private static readonly Color VALUE_BAR_COLOR = new(1f, 0.5f, 0f);
    private static readonly Color CONTROL_KEYFRAME_COLOR = Color.white;
    private static readonly Color CONTROL_CURVE_COLOR = new(1f, 0.5f, 0f);

    private Rect windowRect;
    private int columnCount;
    private float leftX;
    private float rightX;
    private float topY;
    private float bottomY;
    private float paddedWidth;
    private float paddedHeight;
    private float columnWidth;
    private float yMapScale;
    private float yMapOffset;
    private float controlCurveXScale;
    private Texture2D whiteTexture;
    private long[] lastNoteOnTimeInColumn;

    public SequenceRenderer(float windowWidth, float windowHeight, int columnCount) {
        windowRect = new Rect(0f, 0f, windowWidth, windowHeight);
        this.columnCount = columnCount;
        leftX = SIDE_PADDING;
        rightX = windowWidth - SIDE_PADDING;
        topY = TOP_PADDING;
        bottomY = windowHeight - SIDE_PADDING;
        paddedWidth = rightX - leftX;
        paddedHeight = bottomY - topY;
        columnWidth = paddedWidth / columnCount;
        yMapScale = (topY - bottomY) / (TOP_TIME_OFFSET - BOTTOM_TIME_OFFSET);
        yMapOffset = bottomY - yMapScale * BOTTOM_TIME_OFFSET;
        controlCurveXScale = (columnWidth - 2f * CONTROL_CURVE_PADDING) / Constants.MaxEventValue;

        whiteTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.SetPixel(1, 0, Color.white);
        whiteTexture.SetPixel(0, 1, Color.white);
        whiteTexture.SetPixel(1, 1, Color.white);

        lastNoteOnTimeInColumn = new long[columnCount];
    }

    public void Render(SequenceRenderInput input) {
        windowRect = GUI.Window(0, windowRect, _ => DrawWindow(input), "Sequence Editor");
    }

    private void DrawWindow(SequenceRenderInput input) {
        var playState = input.PlayState;
        var editorState = input.EditorState;
        var sequence = input.Sequence;
        var mode = editorState.Mode;
        int cursorIndex = editorState.Column;
        int columnPan = editorState.ColumnPan;

        long time = playState.currentTrackTick;
        float timeAsFloat = playState.currentTrackTick.ToSecondsFloat();
        float[] beatArray = playState.trackData.BeatArray;
        
        DrawRect(leftX, topY, paddedWidth, paddedHeight, BACKING_COLOR, true);
        
        switch (mode) {
            case SequenceEditorMode.Details:
                DrawDetails(editorState);
                break;
            case SequenceEditorMode.OnOffEvents:
                DrawOnOffEvents(time, timeAsFloat, beatArray, sequence, editorState.SelectedIndicesPerColumn[0], cursorIndex, columnPan, editorState.ShowValues);
                break;
            case SequenceEditorMode.ControlCurves:
                DrawControlCurves(time, timeAsFloat, beatArray, sequence, editorState.SelectedIndicesPerColumn, cursorIndex, columnPan, editorState.ShowValues);
                break;
        }
        
        GUI.DragWindow();
    }

    private void DrawDetails(SequenceEditorState state) {
        GUI.Label(new Rect(SIDE_PADDING, TOP_PADDING, paddedWidth, INFO_LABEL_HEIGHT), "Mode: Details");
        DrawField(state.BackgroundField, "background", "Background:", 0);

        var paletteFields = state.PaletteFields;

        for (int i = 0; i < paletteFields.Count; i++)
            DrawField(paletteFields[i], $"palette_{i}", $"Color {i + 1}:", i + 2);
    }

    private void DrawOnOffEvents(long time, float timeAsFloat, float[] beatArray, TrackVisualsEventSequence sequence, List<int> selectedIndices, int cursorIndex, int columnPan, bool showValues) {
        DrawGrid(timeAsFloat, beatArray, cursorIndex, columnPan);
        
        for (int i = 0; i < columnCount; i++)
            lastNoteOnTimeInColumn[i] = long.MinValue;

        var onOffEvents = sequence.OnOffEvents;

        for (int i = 0; i < onOffEvents.Count; i++) {
            var onOffEvent = onOffEvents[i];

            int column = onOffEvent.Index - columnPan;

            if (column < 0 || column >= columnCount)
                continue;

            long eventTime = onOffEvent.Time;
            var eventType = onOffEvent.Type;
            float relativeTime = TICK_TO_TIME * (eventTime - time);
            long lastNoteOnTime = lastNoteOnTimeInColumn[column];

            if (lastNoteOnTime != long.MinValue) {
                float relativeLastNoteOnTime = TICK_TO_TIME * (lastNoteOnTime - time);

                if (relativeLastNoteOnTime <= TOP_TIME_OFFSET && relativeTime >= BOTTOM_TIME_OFFSET)
                    DrawSustainLine(relativeLastNoteOnTime, relativeTime, column);
            }

            if (eventType == OnOffEventType.On)
                lastNoteOnTimeInColumn[column] = eventTime;
            else
                lastNoteOnTimeInColumn[column] = long.MinValue;

            if (relativeTime >= BOTTOM_TIME_OFFSET && relativeTime <= TOP_TIME_OFFSET)
                DrawOnOffEvent(relativeTime, eventType, onOffEvent.Value, column, selectedIndices.Contains(i), showValues);
        }

        for (int i = 0; i < columnCount; i++) {
            long lastNoteOnTime = lastNoteOnTimeInColumn[i];

            if (lastNoteOnTime == long.MinValue)
                continue;

            float relativeLastNoteOnTime = TICK_TO_TIME * (lastNoteOnTime - time);

            if (relativeLastNoteOnTime <= TOP_TIME_OFFSET)
                DrawSustainLine(relativeLastNoteOnTime, TOP_TIME_OFFSET, i);
        }
        
        GUI.Label(new Rect(SIDE_PADDING, TOP_PADDING, paddedWidth, INFO_LABEL_HEIGHT), $"Mode: Events    Index: {cursorIndex:X2}");
    }

    private void DrawControlCurves(long time, float timeAsFloat, float[] beatArray, TrackVisualsEventSequence sequence, List<int>[] selectedIndicesPerColumn, int cursorIndex, int columnPan, bool showValues) {
        DrawGrid(timeAsFloat, beatArray, cursorIndex, columnPan);
        
        for (int i = 0, j = columnPan; i < columnCount; i++, j++) {
            var keyframes = sequence.GetKeyframes(j);
            
            for (int k = 0; k < keyframes.Count - 1; k++) {
                var startKeyframe = keyframes[k];
                var endKeyframe = keyframes[k + 1];
                float relativeStartTime = TICK_TO_TIME * (startKeyframe.Time - time);
                float relativeEndTime = TICK_TO_TIME * (endKeyframe.Time - time);

                if (relativeStartTime <= TOP_TIME_OFFSET && relativeEndTime >= BOTTOM_TIME_OFFSET)
                    DrawCurveSegment(startKeyframe, endKeyframe, time, relativeStartTime, relativeEndTime, i);
            }

            if (keyframes.Count > 0) {
                var keyframe = keyframes[0];
                float relativeTime = TICK_TO_TIME * (keyframe.Time - time);

                if (relativeTime >= BOTTOM_TIME_OFFSET)
                    DrawStraightCurveSegment(BOTTOM_TIME_OFFSET, relativeTime, keyframe.Value, i);

                keyframe = keyframes[keyframes.Count - 1];
                relativeTime = TICK_TO_TIME * (keyframe.Time - time);
                
                if (relativeTime <= TOP_TIME_OFFSET)
                    DrawStraightCurveSegment(relativeTime, TOP_TIME_OFFSET, keyframe.Value, i);
            }
            
            var selectedIndices = selectedIndicesPerColumn[j];

            for (int k = 0; k < keyframes.Count; k++) {
                var keyframe = keyframes[k];
                float relativeTime = TICK_TO_TIME * (keyframe.Time - time);

                if (relativeTime < BOTTOM_TIME_OFFSET)
                    continue;

                if (relativeTime > TOP_TIME_OFFSET)
                    break;

                DrawKeyframe(relativeTime, keyframe.Value, i, selectedIndices.Contains(k), showValues);
            }
        }
        
        GUI.Label(new Rect(SIDE_PADDING, TOP_PADDING, paddedWidth, INFO_LABEL_HEIGHT), $"Mode: Curves    Index: {cursorIndex:X2}");
    }

    private void DrawField(TextFieldState field, string name, string label, int row) {
        float y = TOP_PADDING + DETAILS_START_Y + row * FIELD_HEIGHT;
        
        GUI.Label(new Rect(SIDE_PADDING, y, FIELD_LABEL_WIDTH, FIELD_HEIGHT), label);

        if (GUI.GetNameOfFocusedControl() != name)
            field.RevertDisplayValue();
        
        GUI.SetNextControlName(name);
        field.DisplayValue = GUI.TextField(new Rect(SIDE_PADDING + FIELD_LABEL_WIDTH, y, paddedWidth - FIELD_LABEL_WIDTH, FIELD_HEIGHT), field.DisplayValue);
    }

    private void DrawGrid(float timeAsFloat, float[] beatArray, int cursorIndex, int columnPan) {
        int selectedColumn = cursorIndex - columnPan;

        for (int i = 0; i < columnCount; i += 2) {
            if (i != selectedColumn)
                DrawColumnBox(i, COLUMN_BOX_COLOR);
        }
        
        DrawColumnBox(selectedColumn, COLUMN_BOX_COLOR_SELECTED);

        for (int i = 0; i < beatArray.Length; i++) {
            float beatTime = beatArray[i];
            float relativeBeatTime = beatTime - timeAsFloat;

            if (relativeBeatTime > TOP_TIME_OFFSET)
                break;

            if (relativeBeatTime >= BOTTOM_TIME_OFFSET)
                DrawHorizontalLine(RelativeTimeToY(relativeBeatTime), BEAT_BAR_COLOR);

            if (i >= beatArray.Length - 1)
                continue;

            beatTime = 0.5f * (beatArray[i] + beatArray[i + 1]);
            relativeBeatTime = beatTime - timeAsFloat;

            if (relativeBeatTime >= BOTTOM_TIME_OFFSET)
                DrawHorizontalLine(RelativeTimeToY(relativeBeatTime), BEAT_BAR_COLOR);
        }

        DrawHorizontalLine(yMapOffset, NOW_BAR_COLOR);
    }

    private void DrawHorizontalLine(float y, Color color) => DrawRect(leftX, y, paddedWidth, 1f, color, false);

    private void DrawColumnBox(int column, Color color) => DrawRect(ColumnToX(column), topY, columnWidth, paddedHeight, color, true);

    private void DrawOnOffEvent(float relativeTime, OnOffEventType type, int value, int column, bool selected, bool showValue) {
        float height;
        Color color;

        if (type == OnOffEventType.Off) {
            if (selected)
                height = OFF_EVENT_HEIGHT_SELECTED;
            else
                height = OFF_EVENT_HEIGHT;
            
            color = OFF_EVENT_COLOR;
        }
        else {
            height = ON_EVENT_HEIGHT;
            
            if (selected)
                color = ON_EVENT_COLOR_SELECTED;
            else
                color = ON_EVENT_COLOR;
        }

        float x = ColumnToX(column) + ON_OFF_EVENT_PADDING;
        float y = RelativeTimeToY(relativeTime) - height;
        float width = columnWidth - 2f * ON_OFF_EVENT_PADDING;
        
        DrawRect(x, y, width, height, color, false);

        if (type == OnOffEventType.Off)
            return;
        
        DrawRect(x, y, width * value / Constants.MaxEventValue, VALUE_BAR_HEIGHT, VALUE_BAR_COLOR, false);
        
        if (selected && showValue)
            GUI.Label(new Rect(x, y - VALUE_LABEL_HEIGHT, width, VALUE_LABEL_HEIGHT), $"{value:X2}");
    }

    private void DrawSustainLine(float relativeStartTime, float relativeEndTime, int column) {
        float startY = Mathf.Clamp(RelativeTimeToY(relativeStartTime) - ON_EVENT_HEIGHT, topY, bottomY);
        float endY = Mathf.Clamp(RelativeTimeToY(relativeEndTime), topY, bottomY);
        
        DrawRect(ColumnToX(column) + 0.5f * columnWidth, endY, 1f, startY - endY, ON_EVENT_COLOR, false);
    }

    private void DrawCurveSegment(ControlKeyframe startKeyframe, ControlKeyframe endKeyframe, long time, float relativeStartTime, float relativeEndTime, int column) {
        float startY = Mathf.Clamp(Mathf.Round(RelativeTimeToY(relativeStartTime)), topY, bottomY);
        float endY = Mathf.Clamp(Mathf.Round(RelativeTimeToY(relativeEndTime)), topY, bottomY);
        float sideX = ColumnToX(column) + CONTROL_CURVE_PADDING;

        for (float y = endY; y <= startY; y++) {
            long timeForY = (long) (TIME_TO_TICK * YToRelativeTime(y)) + time;
            float value = ControlKeyframe.Interpolate(startKeyframe, endKeyframe, timeForY);
            float x = sideX + controlCurveXScale * value;
            
            DrawRect(x, y, 1f, 1f, CONTROL_CURVE_COLOR, false);
        }
    }

    private void DrawStraightCurveSegment(float relativeStartTime, float relativeEndTime, int value, int column) {
        float x = ColumnToX(column) + CONTROL_CURVE_PADDING + controlCurveXScale * value;
        float startY = Mathf.Clamp(RelativeTimeToY(relativeStartTime), topY, bottomY);
        float endY = Mathf.Clamp(RelativeTimeToY(relativeEndTime), topY, bottomY);
        
        DrawRect(x, endY, 1f, startY - endY, CONTROL_CURVE_COLOR, false);
    }

    private void DrawKeyframe(float relativeTime, int value, int column, bool selected, bool showValue) {
        float size = selected ? CONTROL_KEYFRAME_SIZE_SELECTED : CONTROL_KEYFRAME_SIZE;
        float offset = -0.5f * (size - 1f);
        float sideX = ColumnToX(column) + CONTROL_CURVE_PADDING;
        float x = sideX + controlCurveXScale * value + offset;
        float y = RelativeTimeToY(relativeTime) + offset;
        
        DrawRect(x, y, size, size, CONTROL_KEYFRAME_COLOR, false);

        if (selected && showValue)
            GUI.Label(new Rect(sideX, y - VALUE_LABEL_HEIGHT, columnWidth - 2f * CONTROL_CURVE_PADDING, VALUE_LABEL_HEIGHT), $"{value:X2}");
    }

    private void DrawRect(float x, float y, float width, float height, Color color, bool alphaBlend)
        => GUI.DrawTexture(new Rect(x, y, width, height), whiteTexture, ScaleMode.StretchToFill, alphaBlend, 0, color, 0, 0);

    private float ColumnToX(int index) => columnWidth * index + leftX;

    private float RelativeTimeToY(float time) => yMapScale * time + yMapOffset;

    private float YToRelativeTime(float y) => (y - yMapOffset) / yMapScale;
}