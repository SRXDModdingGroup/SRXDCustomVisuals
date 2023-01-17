using UnityEngine;

namespace SRXDCustomVisuals.Plugin;

public class SequenceRenderer {
    private Rect windowRect;

    public SequenceRenderer(float windowWidth, float windowHeight) {
        windowRect = new Rect(0f, 0f, windowWidth, windowHeight);
    }

    public void Render(RenderInfo info) => windowRect = GUI.Window(0, windowRect, id => DrawWindow(id, info), "Sequence Editor");

    private static void DrawWindow(int windowId, RenderInfo info) {
        GUI.DragWindow();
    }
}