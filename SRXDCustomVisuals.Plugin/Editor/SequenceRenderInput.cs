namespace SRXDCustomVisuals.Plugin; 

public class SequenceRenderInput {
    public TrackVisualsProject Sequence { get; }
    
    public VisualsBackground Background { get; }

    public PlayState PlayState { get; }

    public SequenceEditorState EditorState { get; }
    
    public SequenceRenderInput(TrackVisualsProject sequence, VisualsBackground background, PlayState playState, SequenceEditorState editorState) {
        Background = background;
        PlayState = playState;
        EditorState = editorState;
        Sequence = sequence;
    }
}