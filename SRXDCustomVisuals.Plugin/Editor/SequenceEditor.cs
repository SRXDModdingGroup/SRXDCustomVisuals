using System;
using UnityEngine;
using Input = UnityEngine.Input;

namespace SRXDCustomVisuals.Plugin;

public class SequenceEditor : MonoBehaviour {
    private const float WINDOW_WIDTH = 800;
    private const float WINDOW_HEIGHT = 600;
    private const int COLUMN_COUNT = 16;
    
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
        if (Input.GetKeyDown(KeyCode.F1)) {
            Visible = !Visible;
            
            return;
        }
        
        if (!Visible)
            return;
        
        long previousTime = state.Time;
        
        CheckInputs();
        state.Time = playState.currentTrackTick;
    }

    public void Exit() {
        sequence = new TrackVisualsEventSequence();
        playState = null;
    }

    private void CheckInputs() {
        bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        int direction = 0;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            direction++;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            direction--;

        if (direction != 0) {
            if (altPressed)
                direction *= 8;
            
            MoveTime(direction);
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
            direction++;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            direction--;
        
        if (direction != 0) {
            if (altPressed)
                direction *= 8;
            
            MoveCursorIndex(direction);
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
            direction++;

        if (Input.GetKeyDown(KeyCode.PageDown))
            direction--;
        
        if (direction != 0) {
            if (altPressed)
                direction *= 8;
            
            ChangeChannel(direction);
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            PlaceOnOffEvent(OnOffEventType.On);
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            PlaceOnOffEvent(OnOffEventType.Off);
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            PlaceOnOffEvent(OnOffEventType.OnOff);
            
            return;
        }
    }
    
    private void MoveCursorIndex(int direction) {
        state.CursorIndex = Mod(state.CursorIndex + direction, 256);
        state.ColumnPan = Mathf.Clamp(state.ColumnPan, state.CursorIndex - (COLUMN_COUNT - 2), state.CursorIndex - 1);
        state.ColumnPan = Mathf.Clamp(state.ColumnPan, 0, 240);
    }

    private void ChangeChannel(int direction) => state.CurrentChannel = Mod(state.CurrentChannel + direction, 256);

    private void PlaceOnOffEvent(OnOffEventType type) {
        sequence.Channels[state.CurrentChannel].OnOffEvents.InsertSorted(new OnOffEvent(state.Time, type, (byte) state.CursorIndex, 255));
    }

    private static void MoveTime(int direction) {
        var trackEditor = Track.Instance.trackEditor;
        
        trackEditor.SetCurrentTrackTime(trackEditor.GetQuantizedMoveTime(direction), false);
    }

    private static int Mod(int a, int b) => (a % b + b) % b;
}