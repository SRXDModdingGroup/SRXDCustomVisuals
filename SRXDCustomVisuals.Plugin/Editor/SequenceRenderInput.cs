namespace SRXDCustomVisuals.Plugin; 

public class SequenceRenderInput {
    public PlayState PlayState { get; }
    
    public SequenceEditorState EditorState { get; }
    
    public TrackVisualsEventSequence Sequence { get; }

    public SequenceRenderInput(PlayState playState, SequenceEditorState editorState, TrackVisualsEventSequence sequence) {
        PlayState = playState;
        EditorState = editorState;
        Sequence = sequence;
    }
}