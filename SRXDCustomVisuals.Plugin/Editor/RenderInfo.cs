namespace SRXDCustomVisuals.Plugin; 

public class RenderInfo {
    public PlayState PlayState { get; }
    
    public SequenceEditorState EditorState { get; }
    
    public TrackVisualsEventSequence Sequence { get; }

    public RenderInfo(PlayState playState, SequenceEditorState editorState, TrackVisualsEventSequence sequence) {
        PlayState = playState;
        EditorState = editorState;
        Sequence = sequence;
    }
}