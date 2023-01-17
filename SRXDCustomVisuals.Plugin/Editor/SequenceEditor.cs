using System;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin;

public class SequenceEditor : MonoBehaviour {
    public static SequenceEditor Instance { get; private set; }

    public bool Visible { get; set; }

    private SequenceEditorState state;
    private SequenceRenderer renderer;
    private PlayState playState;
    private TrackVisualsEventSequence sequence;

    private void Awake() {
        Instance = this;
        state = new SequenceEditorState();
        renderer = new SequenceRenderer(800, 400);
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
        
    }

    public void Exit() {
        sequence = new TrackVisualsEventSequence();
        playState = null;
    }
}