﻿using UnityEngine;

namespace SRXDCustomVisuals.Plugin;

public class SequenceRenderer {
    private const float TICK_TO_TIME = 0.00001f;
    private const float BOTTOM_TIME_OFFSET = -0.25f;
    private const float TOP_TIME_OFFSET = 1f;
    private const float SIDE_PADDING = 8f;
    private const float TOP_PADDING = 20f;
    private const float ON_EVENT_HEIGHT = 4f;
    private const float OFF_EVENT_HEIGHT = 2f;
    private const float ON_EVENT_HEIGHT_SELECTED = 6f;
    private const float OFF_EVENT_HEIGHT_SELECTED = 4f;
    private const float ON_OFF_EVENT_PADDING = 4f;
    private static readonly Color BEAT_BAR_COLOR = new(0.5f, 0.5f, 0.5f);
    private static readonly Color NOW_BAR_COLOR = Color.white;
    private static readonly Color COLUMN_BOX_COLOR = new(0.5f, 1f, 1f, 0.2f);
    private static readonly Color ON_EVENT_COLOR = new(0f, 0.5f, 1f);
    private static readonly Color OFF_EVENT_COLOR = Color.white;
    private static readonly Color ON_EVENT_COLOR_SELECTED = new(0.5f, 0.75f, 1f);
    private static readonly Color OFF_EVENT_COLOR_SELECTED = Color.white;

    private Rect windowRect;
    private int columnCount;
    private float columnWidth;
    private float yMapScale;
    private float yMapOffset;
    private float leftX;
    private float rightX;
    private float topY;
    private float bottomY;
    private float paddedWidth;
    private float paddedHeight;
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

        whiteTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.SetPixel(1, 0, Color.white);
        whiteTexture.SetPixel(0, 1, Color.white);
        whiteTexture.SetPixel(1, 1, Color.white);

        lastNoteOnTimeInColumn = new long[columnCount];
    }

    public void Render(RenderInfo info) => windowRect = GUI.Window(0, windowRect, _ => DrawWindow(info), "Sequence Editor");

    private void DrawWindow(RenderInfo info) {
        var playState = info.PlayState;
        
        var editorState = info.EditorState;
        int cursorIndex = editorState.CursorIndex;
        int columnPan = editorState.ColumnPan;
        var selectedIndices = editorState.SelectedIndices;
        
        var sequence = info.Sequence;
        var onOffEvents = sequence.OnOffEvents;
        
        long time = playState.currentTrackTick;
        float timeAsFloat = playState.currentTrackTick.ToSecondsFloat();
        float[] beatArray = playState.trackData.BeatArray;
        
        DrawColumnBox(cursorIndex - columnPan);

        foreach (float beatTime in beatArray) {
            float relativeBeatTime = beatTime - timeAsFloat;
            
            if (relativeBeatTime > TOP_TIME_OFFSET)
                break;
            
            if (relativeBeatTime < BOTTOM_TIME_OFFSET)
                continue;
            
            DrawHorizontalLine(RelativeTimeToY(relativeBeatTime), BEAT_BAR_COLOR);
        }
        
        DrawHorizontalLine(yMapOffset, NOW_BAR_COLOR);

        for (int i = 0; i < columnCount; i++)
            lastNoteOnTimeInColumn[i] = long.MinValue;

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
                DrawOnOffEvent(eventType, relativeTime, column, selectedIndices.Contains(i));
        }

        for (int i = 0; i < columnCount; i++) {
            long lastNoteOnTime = lastNoteOnTimeInColumn[i];

            if (lastNoteOnTime == long.MinValue)
                continue;
            
            float relativeLastNoteOnTime = TICK_TO_TIME * (lastNoteOnTime - time);

            if (relativeLastNoteOnTime <= TOP_TIME_OFFSET)
                DrawSustainLine(relativeLastNoteOnTime, TOP_TIME_OFFSET, i);
        }
        
        GUI.Label(new Rect(SIDE_PADDING, TOP_PADDING, paddedWidth, 20f), $"Index: {cursorIndex:X2}");
        GUI.DragWindow();
    }

    private void DrawHorizontalLine(float y, Color color) => DrawRect(leftX, y, paddedWidth, 1f, color, false);

    private void DrawColumnBox(int column) => DrawRect(ColumnToX(column), topY, columnWidth, paddedHeight, COLUMN_BOX_COLOR, true);

    private void DrawOnOffEvent(OnOffEventType type, float relativeTime, int column, bool selected) {
        float height;
        Color color;

        if (type == OnOffEventType.Off) {
            if (selected) {
                height = OFF_EVENT_HEIGHT_SELECTED;
                color = OFF_EVENT_COLOR_SELECTED;
            }
            else {
                height = OFF_EVENT_HEIGHT;
                color = OFF_EVENT_COLOR;
            }
        }
        else {
            if (selected) {
                height = ON_EVENT_HEIGHT_SELECTED;
                color = ON_EVENT_COLOR_SELECTED;
            }
            else {
                height = ON_EVENT_HEIGHT;
                color = ON_EVENT_COLOR;
            }
        }
        
        DrawRect(
            ColumnToX(column) + ON_OFF_EVENT_PADDING,
            RelativeTimeToY(relativeTime) - height,
            columnWidth - 2f * ON_OFF_EVENT_PADDING,
            height,
            color,
            false);
    }

    private void DrawSustainLine(float relativeStartTime, float relativeEndTime, int column) {
        float startY = Mathf.Clamp(RelativeTimeToY(relativeStartTime), topY, bottomY);
        float endY = Mathf.Clamp(RelativeTimeToY(relativeEndTime), topY, bottomY);
        
        DrawRect(ColumnToX(column) + 0.5f * columnWidth, endY, 1f, startY - endY, ON_EVENT_COLOR, false);
    }

    private void DrawRect(float x, float y, float width, float height, Color color, bool alphaBlend)
        => GUI.DrawTexture(new Rect(x, y, width, height), whiteTexture, ScaleMode.StretchToFill, alphaBlend, 0, color, 0, 0);

    private float ColumnToX(int index) => columnWidth * index + leftX;

    private float RelativeTimeToY(float time) => yMapScale * time + yMapOffset;
}