using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin;

public class SequenceRenderer {
    private const float TIME_TO_TICK = 100000f;
    private const float TICK_TO_TIME = 0.00001f;
    private const float TIMESPAN = 1.25f;
    private const float CURSOR_TIME = 0.25f;
    private const int SIDE_PADDING = 8;
    private const int TOP_PADDING = 20;
    private const int ON_EVENT_HEIGHT = 6;
    private const int OFF_EVENT_HEIGHT = 2;
    private const int OFF_EVENT_HEIGHT_SELECTED = 4;
    private const int ON_OFF_EVENT_PADDING = 4;
    private const int CONTROL_KEYFRAME_SIZE = 5;
    private const int CONTROL_KEYFRAME_SIZE_SELECTED = 9;
    private const int CONTROL_CURVE_PADDING = 4;
    private const int VALUE_BAR_HEIGHT = 2;
    private const int INFO_LABEL_HEIGHT = 20;
    private const int VALUE_LABEL_HEIGHT = 20;
    private const int FIELD_HEIGHT = 20;
    private const int FIELD_LABEL_WIDTH = 100;
    private const int FIELD_PADDING = 10;
    private const int DETAILS_START_Y = 40;
    private const int COLUMN_LABEL_SPACING = 20;
    private static readonly Color BACKING_COLOR = new(0f, 0f, 0f, 0.75f);
    private static readonly Color BEAT_BAR_COLOR = new(0.5f, 0.5f, 0.5f);
    private static readonly Color COLUMN_BOX_COLOR = new(0.25f, 0.25f, 0.25f, 0.1f);
    private static readonly Color COLUMN_BOX_COLOR_SELECTED = new(0.5f, 1f, 1f, 0.1f);
    private static readonly Color CURSOR_BAR_COLOR = Color.white;
    private static readonly Color ON_EVENT_COLOR = new(0f, 0.4f, 0.8f);
    private static readonly Color OFF_EVENT_COLOR = Color.white;
    private static readonly Color ON_EVENT_COLOR_SELECTED = new(0.75f, 0.875f, 1f);
    private static readonly Color VALUE_BAR_COLOR = new(1f, 0.5f, 0f);
    private static readonly Color CONTROL_KEYFRAME_COLOR = Color.white;
    private static readonly Color CONTROL_CURVE_COLOR = new(1f, 0.5f, 0f);
    private static readonly GUIStyle COLUMN_LABEL_STYLE = new() {
        normal = new GUIStyleState { textColor = new Color(1f, 1f, 1f, 0.15f) },
        fontSize = 20,
        alignment = TextAnchor.MiddleLeft
    };

    private Rect windowRect;
    private int columnCount;
    private int canvasLeft;
    private int canvasRight;
    private int canvasTop;
    private int canvasBottom;
    private int canvasWidth;
    private int canvasHeight;
    private int columnWidth;
    private int cursorY;
    private float secondsPerPixel;
    private float pixelsPerSecond;
    private Texture2D whiteTexture;
    private Texture2D controlCurveTexture;
    private int[] controlCurveActivePixels;

    public SequenceRenderer(int windowWidth, int windowHeight, int columnCount) {
        windowRect = new Rect(0f, 0f, windowWidth, windowHeight);
        this.columnCount = columnCount;
        canvasLeft = SIDE_PADDING;
        canvasRight = windowWidth - SIDE_PADDING;
        canvasTop = TOP_PADDING;
        canvasBottom = windowHeight - SIDE_PADDING;
        canvasWidth = canvasRight - canvasLeft;
        canvasHeight = canvasBottom - canvasTop;
        columnWidth = canvasWidth / columnCount;
        secondsPerPixel = TIMESPAN / canvasHeight;
        pixelsPerSecond = canvasHeight / TIMESPAN;
        cursorY = RelativeTimeToY(CURSOR_TIME);

        whiteTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.SetPixel(1, 0, Color.white);
        whiteTexture.SetPixel(0, 1, Color.white);
        whiteTexture.SetPixel(1, 1, Color.white);

        controlCurveTexture = new Texture2D(columnWidth - 2 * CONTROL_CURVE_PADDING, canvasHeight, TextureFormat.RGBA32, false);

        var pixelData = controlCurveTexture.GetPixelData<Color32>(0);

        for (int i = 0; i < pixelData.Length; i++)
            pixelData[i] = new Color32(0, 0, 0, 0);
        
        controlCurveTexture.Apply();
        controlCurveActivePixels = new int[controlCurveTexture.height];
    }

    public void Render(SequenceRenderInput input) {
        windowRect = GUI.Window(0, windowRect, _ => DrawWindow(input), "Sequence Editor");
    }

    private void DrawWindow(SequenceRenderInput input) {
        var playState = input.PlayState;
        var editorState = input.EditorState;
        var sequence = input.Sequence;
        var background = input.Background;
        var mode = editorState.Mode;
        int cursorIndex = editorState.Column;
        int columnPan = editorState.ColumnPan;

        long time = (long) playState.currentTrackTick - (long) (TIME_TO_TICK * CURSOR_TIME);
        float timeAsFloat = playState.currentTrackTick.ToSecondsFloat() - CURSOR_TIME;
        float[] beatArray = playState.trackData.BeatArray;
        
        DrawRect(canvasLeft, canvasTop, canvasWidth, canvasHeight, BACKING_COLOR, true);
        
        switch (mode) {
            case SequenceEditorMode.Details:
                DrawDetails(editorState, sequence.Palette);
                break;
            case SequenceEditorMode.OnOffEvents when Event.current.type == EventType.Repaint:
                DrawOnOffEvents(time, timeAsFloat, beatArray, sequence.OnOffEvents, background.EventLabels, editorState.SelectedIndicesPerColumn, cursorIndex, columnPan, editorState.ShowValues);
                break;
            case SequenceEditorMode.ControlCurves when Event.current.type == EventType.Repaint:
                DrawControlCurves(time, timeAsFloat, beatArray, sequence.ControlCurves, background.CurveLabels, editorState.SelectedIndicesPerColumn, cursorIndex, columnPan, editorState.ShowValues);
                break;
        }
        
        GUI.DragWindow();
    }

    private void DrawDetails(SequenceEditorState state, IReadOnlyList<Color32> palette) {
        DrawModeLabel("Mode: Details");
        DrawSimpleField(state.BackgroundField, "background", "Background:", 0);

        var paletteFields = state.PaletteFields;

        for (int i = 0; i < paletteFields.Count && i < palette.Count; i++)
            DrawPaletteField(paletteFields[i], $"palette_{i}", $"Color {i + 1}:", i + 2, palette[i].ToColor());
    }

    private void DrawOnOffEvents(long time, float timeAsFloat, float[] beatArray, IReadOnlySequence<OnOffEvent> onOffEvents,
        IReadOnlyList<string> labels, List<int>[] selectedIndicesPerColumn, int cursorIndex, int firstColumnIndex, bool showValues) {
        DrawGrid(timeAsFloat, beatArray, cursorIndex, firstColumnIndex);

        for (int i = 0, j = firstColumnIndex; i < columnCount && j < labels.Count; i++, j++)
            DrawColumnLabel(i, labels[j]);

        for (int i = 0, j = firstColumnIndex; i < columnCount; i++, j++)
            DrawOnOffEventsInColumn(onOffEvents.GetElementsInColumn(j), selectedIndicesPerColumn[j], time, i, showValues);
        
        DrawModeLabel($"Mode: Events    Index: {cursorIndex:X2}");
    }

    private void DrawControlCurves(long time, float timeAsFloat, float[] beatArray, IReadOnlySequence<ControlKeyframe> controlCurves,
        IReadOnlyList<string> labels, List<int>[] selectedIndicesPerColumn, int cursorIndex, int columnPan, bool showValues) {
        DrawGrid(timeAsFloat, beatArray, cursorIndex, columnPan);
        
        for (int i = 0, j = columnPan; i < columnCount && j < labels.Count; i++, j++)
            DrawColumnLabel(i, labels[j]);
        
        for (int i = 0, j = columnPan; i < columnCount; i++, j++)
            DrawControlCurve(controlCurves.GetElementsInColumn(j), selectedIndicesPerColumn[j], time, i, showValues);

        DrawModeLabel($"Mode: Curves    Index: {cursorIndex:X2}");
    }

    private void DrawModeLabel(string text) => GUI.Label(new Rect(SIDE_PADDING + FIELD_PADDING, TOP_PADDING, canvasWidth, INFO_LABEL_HEIGHT), text);

    private void DrawSimpleField(TextFieldState field, string name, string label, int row) {
        const int x = SIDE_PADDING + FIELD_PADDING;
        int y = TOP_PADDING + DETAILS_START_Y + row * FIELD_HEIGHT;
        
        GUI.Label(new Rect(x, y, FIELD_LABEL_WIDTH, FIELD_HEIGHT), label);
        DrawField(field, name, x + FIELD_LABEL_WIDTH, y, canvasWidth - FIELD_LABEL_WIDTH - 2 * FIELD_PADDING, FIELD_HEIGHT);
    }

    private void DrawPaletteField(TextFieldState field, string name, string label, int row, Color color) {
        const int x = SIDE_PADDING + FIELD_PADDING;
        int y = TOP_PADDING + DETAILS_START_Y + row * FIELD_HEIGHT;
        
        GUI.Label(new Rect(x, y, FIELD_LABEL_WIDTH, FIELD_HEIGHT), label);
        DrawField(field, name, x + FIELD_LABEL_WIDTH, y, canvasWidth - FIELD_LABEL_WIDTH - 2 * FIELD_PADDING - FIELD_HEIGHT, FIELD_HEIGHT);
        DrawRect(canvasRight - FIELD_PADDING - FIELD_HEIGHT, y, FIELD_HEIGHT, FIELD_HEIGHT, color, false);
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

            if (relativeBeatTime > TIMESPAN)
                break;

            if (relativeBeatTime >= 0f)
                DrawHorizontalLine(RelativeTimeToY(relativeBeatTime), BEAT_BAR_COLOR);

            if (i >= beatArray.Length - 1)
                continue;

            beatTime = 0.5f * (beatArray[i] + beatArray[i + 1]);
            relativeBeatTime = beatTime - timeAsFloat;

            if (relativeBeatTime >= 0f)
                DrawHorizontalLine(RelativeTimeToY(relativeBeatTime), BEAT_BAR_COLOR);
        }

        DrawHorizontalLine(cursorY, CURSOR_BAR_COLOR);
    }

    private void DrawHorizontalLine(int y, Color color) => DrawRect(canvasLeft, y, canvasWidth, 1, color, false);

    private void DrawColumnBox(int column, Color color) => DrawRect(ColumnToX(column), canvasTop, columnWidth, canvasHeight, color, false);

    private void DrawColumnLabel(int column, string text) {
        int y = cursorY - COLUMN_LABEL_SPACING;

        GUIUtility.RotateAroundPivot(-90f, Vector2.zero);
        GUI.matrix = Matrix4x4.Translate(new Vector3(ColumnToX(column), y)) * GUI.matrix;
        GUI.Label(new Rect(0f, 0f, y, columnWidth), text, COLUMN_LABEL_STYLE);
        GUI.matrix = Matrix4x4.identity;
    }

    private void DrawOnOffEvent(float relativeTime, OnOffEventType type, int value, int column, bool selected, bool showValue) {
        int height;
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

        int x = ColumnToX(column) + ON_OFF_EVENT_PADDING;
        int y = RelativeTimeToY(relativeTime) - height;
        int width = columnWidth - 2 * ON_OFF_EVENT_PADDING;
        
        DrawRect(x, y, width, height, color, false);

        if (type == OnOffEventType.Off)
            return;
        
        DrawRect(x, y, Mathf.RoundToInt(width * (float) value / Constants.MaxEventValue), VALUE_BAR_HEIGHT, VALUE_BAR_COLOR, false);
        
        if (selected && showValue)
            GUI.Label(new Rect(x, y - VALUE_LABEL_HEIGHT, width, VALUE_LABEL_HEIGHT), $"{value:X2}");
    }

    private void DrawSustainLine(float relativeStartTime, float relativeEndTime, int column) {
        int startY = Mathf.Clamp(RelativeTimeToY(relativeStartTime) - ON_EVENT_HEIGHT, canvasTop, canvasBottom);
        int endY = Mathf.Clamp(RelativeTimeToY(relativeEndTime), canvasTop, canvasBottom);
        
        DrawRect(ColumnToX(column) + columnWidth / 2, endY, 1, startY - endY, ON_EVENT_COLOR, false);
    }

    private void DrawOnOffEventsInColumn(IReadOnlyList<OnOffEvent> onOffEvents, List<int> selectedIndices, long time, int column, bool showValues) {
        long lastEventOnTime = long.MinValue;
        float relativeLastNoteOnTime;

        foreach (var onOffEvent in onOffEvents) {
            float relativeTime = TICK_TO_TIME * (onOffEvent.Time - time);

            if (lastEventOnTime != long.MinValue) {
                relativeLastNoteOnTime = TICK_TO_TIME * (lastEventOnTime - time);

                if (relativeLastNoteOnTime <= TIMESPAN && relativeTime >= 0f)
                    DrawSustainLine(relativeLastNoteOnTime, relativeTime, column);
            }

            if (onOffEvent.Type == OnOffEventType.On)
                lastEventOnTime = onOffEvent.Time;
            else
                lastEventOnTime = long.MinValue;

            if (relativeTime >= 0f && relativeTime <= TIMESPAN)
                DrawOnOffEvent(relativeTime, onOffEvent.Type, onOffEvent.Value, column, selectedIndices.Contains(column), showValues);
        }
        
        if (lastEventOnTime == long.MinValue)
            return;

        relativeLastNoteOnTime = TICK_TO_TIME * (lastEventOnTime - time);

        if (relativeLastNoteOnTime <= TIMESPAN)
            DrawSustainLine(relativeLastNoteOnTime, TIMESPAN, column);
    }

    private void DrawControlCurve(IReadOnlyList<ControlKeyframe> keyframes, List<int> selectedIndices, long time, int column, bool showValues) {
        if (keyframes.Count == 0)
            return;
        
        var pixelData = controlCurveTexture.GetPixelData<Color32>(0);
        var controlCurveColor32 = (Color32) CONTROL_CURVE_COLOR;
        int currentKeyframeIndex = -1;
        float scale = (float) (controlCurveTexture.width - 1) / Constants.MaxEventValue;

        foreach (int index in controlCurveActivePixels)
            pixelData[index] = new Color32(0, 0, 0, 0);

        for (int row = 0, firstIndex = 0; row < controlCurveTexture.height; row++, firstIndex += controlCurveTexture.width) {
            long timeForRow = time + (long) (TIME_TO_TICK * secondsPerPixel * row);

            while (currentKeyframeIndex < keyframes.Count - 1 && keyframes[currentKeyframeIndex + 1].Time <= timeForRow)
                currentKeyframeIndex++;

            float value;

            if (currentKeyframeIndex == -1)
                value = keyframes[0].Value;
            else if (currentKeyframeIndex == keyframes.Count - 1)
                value = keyframes[currentKeyframeIndex].Value;
            else
                value = ControlKeyframe.Interpolate(keyframes[currentKeyframeIndex], keyframes[currentKeyframeIndex + 1], timeForRow);

            int index = firstIndex + Mathf.RoundToInt(scale * value);
            
            pixelData[index] = controlCurveColor32;
            controlCurveActivePixels[row] = index;
        }
        
        controlCurveTexture.Apply();
        GUI.DrawTexture(new Rect(ColumnToX(column) + CONTROL_CURVE_PADDING, canvasTop, controlCurveTexture.width, controlCurveTexture.height), controlCurveTexture, ScaleMode.StretchToFill, true);
        
        for (int k = 0; k < keyframes.Count; k++) {
            var keyframe = keyframes[k];
            float relativeTime = TICK_TO_TIME * (keyframe.Time - time);

            if (relativeTime < 0f)
                continue;

            if (relativeTime > TIMESPAN)
                break;

            DrawKeyframe(relativeTime, keyframe.Value, column, selectedIndices.Contains(k), showValues);
        }
    }

    private void DrawKeyframe(float relativeTime, int value, int column, bool selected, bool showValue) {
        int size = selected ? CONTROL_KEYFRAME_SIZE_SELECTED : CONTROL_KEYFRAME_SIZE;
        int sideX = ColumnToX(column) + CONTROL_CURVE_PADDING;
        int x = sideX + Mathf.RoundToInt((float) (controlCurveTexture.width - 1) * value / Constants.MaxEventValue) - size / 2;
        int y = RelativeTimeToY(relativeTime) - size / 2;
        
        DrawRect(x, y, size, size, CONTROL_KEYFRAME_COLOR, false);

        if (selected && showValue)
            GUI.Label(new Rect(sideX, y - VALUE_LABEL_HEIGHT, columnWidth - 2f * CONTROL_CURVE_PADDING, VALUE_LABEL_HEIGHT), $"{value:X2}");
    }

    private void DrawRect(int x, int y, int width, int height, Color color, bool alphaBlend)
        => GUI.DrawTexture(new Rect(x, y, width, height), whiteTexture, ScaleMode.StretchToFill, alphaBlend, 0, color, 0, 0);

    private int ColumnToX(int column) => columnWidth * column + canvasLeft;

    private int RelativeTimeToY(float relativeTime) => canvasBottom - Mathf.RoundToInt(pixelsPerSecond * relativeTime);

    private static void DrawField(TextFieldState field, string name, int x, int y, int width, int height) {
        if (GUI.GetNameOfFocusedControl() != name)
            field.RevertDisplayValue();
        
        GUI.SetNextControlName(name);
        field.DisplayValue = GUI.TextField(new Rect(x, y, width, height), field.DisplayValue);
    }
}