using UnityEngine;

namespace SRXDCustomVisuals.Plugin;

public class SequenceRenderer {
    private const float BOTTOM_TIME_OFFSET = -0.1f;
    private const float TOP_TIME_OFFSET = 1f;
    private const float SIDE_PADDING = 8f;
    private const float TOP_PADDING = 20f;
    private static readonly Color BEAT_BAR_COLOR = new(0.5f, 0.5f, 0.5f);
    private static readonly Color NOW_BAR_COLOR = Color.white;

    private Rect windowRect;
    private float yMapScale;
    private float yMapOffset;
    private float paddedWidth;
    private Texture2D whiteTexture;

    public SequenceRenderer(float windowWidth, float windowHeight) {
        windowRect = new Rect(0f, 0f, windowWidth, windowHeight);
        paddedWidth = windowWidth - 2f * SIDE_PADDING;

        float toMin = windowHeight - SIDE_PADDING;
        
        yMapScale = (TOP_PADDING - toMin) / (TOP_TIME_OFFSET - BOTTOM_TIME_OFFSET);
        yMapOffset = toMin - yMapScale * BOTTOM_TIME_OFFSET;

        whiteTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.SetPixel(1, 0, Color.white);
        whiteTexture.SetPixel(0, 1, Color.white);
        whiteTexture.SetPixel(1, 1, Color.white);
    }

    public void Render(RenderInfo info) => windowRect = GUI.Window(0, windowRect, _ => DrawWindow(info), "Sequence Editor");

    private void DrawWindow(RenderInfo info) {
        var playState = info.PlayState;
        var editorState = info.EditorState;
        long time = playState.currentTrackTick;
        float timeAsFloat = playState.currentTrackTick.ToSecondsFloat();
        float[] beatArray = playState.trackData.BeatArray;

        foreach (float beatTime in beatArray) {
            float relativeBeatTime = beatTime - timeAsFloat;
            
            if (relativeBeatTime > TOP_TIME_OFFSET)
                break;
            
            if (relativeBeatTime < BOTTOM_TIME_OFFSET)
                continue;
            
            DrawHorizontalLine(TimeToY(relativeBeatTime), BEAT_BAR_COLOR);
        }
        
        DrawHorizontalLine(yMapOffset, NOW_BAR_COLOR);
        GUI.Label(new Rect(SIDE_PADDING, TOP_PADDING, paddedWidth, 20f), $"Channel: {editorState.CurrentChannel:000}    Index: {editorState.CursorIndex:000}");
        GUI.DragWindow();
    }
    
    private void DrawHorizontalLine(float y, Color color) => GUI.DrawTexture(new Rect(SIDE_PADDING, y, paddedWidth, 1f), whiteTexture, ScaleMode.ScaleAndCrop, false, 0, color, 0, 0);

    private float TimeToY(float time) => yMapScale * time + yMapOffset;
}