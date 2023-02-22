namespace SRXDCustomVisuals.Plugin; 

public class RenderInput {
    public PlayState PlayState { get; }
    
    public SequenceEditorState EditorState { get; }
    
    public TrackVisualsEventSequence Sequence { get; }

    public RenderInput(PlayState playState, SequenceEditorState editorState, TrackVisualsEventSequence sequence) {
        PlayState = playState;
        EditorState = editorState;
        Sequence = sequence;
    }
}