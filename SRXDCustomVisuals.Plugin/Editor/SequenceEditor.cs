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
        if (Input.GetKeyDown(KeyCode.UpArrow))
            MoveTime(1);
        
        if (Input.GetKeyDown(KeyCode.DownArrow))
            MoveTime(-1);
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MovePosition(1);
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MovePosition(-1);
        
        if (Input.GetKeyDown(KeyCode.PageUp))
            ChangeChannel(1);
        
        if (Input.GetKeyDown(KeyCode.PageDown))
            ChangeChannel(-1);
    }

    public void Exit() {
        sequence = new TrackVisualsEventSequence();
        playState = null;
    }

    private void MoveTime(int direction) {
        if (AltPressed())
            direction *= 8;

        var trackEditor = Track.Instance.trackEditor;
        long tickBefore = playState.currentTrackTick;
        
        trackEditor.SetCurrentTrackTime(trackEditor.GetQuantizedMoveTime(direction), false);
        
        long tickAfter = playState.currentTrackTick;
    }
    
    private void MovePosition(int direction) {
        if (AltPressed())
            direction *= 8;
        
        state.CursorIndex += direction;

        if (state.CursorIndex < 0)
            state.CursorIndex = 0;
        else if (state.CursorIndex > 255)
            state.CursorIndex = 255;
    }

    private void ChangeChannel(int direction) {
        if (AltPressed())
            direction *= 8;
        
        state.CurrentChannel += direction;

        if (state.CurrentChannel < 0)
            state.CurrentChannel = 0;
        else if (state.CurrentChannel > 255)
            state.CurrentChannel = 255;
    }

    private static bool AltPressed() => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
}