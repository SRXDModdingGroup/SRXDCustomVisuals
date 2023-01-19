using System;
using UnityEngine;
using Input = UnityEngine.Input;

namespace SRXDCustomVisuals.Plugin;

public class SequenceEditor : MonoBehaviour {
    private const float TICK_TO_TIME = 0.00001f;
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
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
            direction++;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            direction--;
        
        if (direction != 0) {
            MoveCursorIndex(
                direction,
                Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt),
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            PlaceOnOffEventAtCursor(OnOffEventType.OnOff);
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            PlaceOnOffEventAtCursor(OnOffEventType.On);
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            PlaceOnOffEventAtCursor(OnOffEventType.Off);
            
            return;
        }
    }
    
    private void MoveCursorIndex(int direction, bool largeMovement, bool moveSelected) {
        if (largeMovement)
            direction *= 8;
        
        state.CursorIndex = Mod(state.CursorIndex + direction, 256);
        state.ColumnPan = Mathf.Clamp(state.ColumnPan, state.CursorIndex - (COLUMN_COUNT - 2), state.CursorIndex - 1);
        state.ColumnPan = Mathf.Clamp(state.ColumnPan, 0, 240);
    }

    private void PlaceOnOffEventAtCursor(OnOffEventType type) {
        sequence.OnOffEvents.InsertSorted(new OnOffEvent(state.Time, type, state.CursorIndex, 255));
    }

    private void MoveTime(int direction, bool largeMovement, bool smallMovement, bool moveSelected) {
        if (largeMovement)
            direction *= 8;

        var trackEditor = Track.Instance.trackEditor;

        if (smallMovement || moveSelected) {
            float directionFloat = 0.125f * direction;

            if (smallMovement)
                directionFloat *= 0.125f;
            
            trackEditor.SetCurrentTrackTime(playState.trackData.GetTimeOffsetByTicks(TICK_TO_TIME * state.Time, directionFloat), false);
        }
        else
            trackEditor.SetCurrentTrackTime(trackEditor.GetQuantizedMoveTime(TICK_TO_TIME * state.Time, direction), false);
    }

    private static int Mod(int a, int b) => (a % b + b) % b;
}