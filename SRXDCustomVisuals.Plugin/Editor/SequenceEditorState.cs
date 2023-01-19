using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class SequenceEditorState {
    public long Time { get; set; }
    
    public int CursorIndex { get; set; }
    
    public int ColumnPan { get; set; }
    
    public long SelectionStartTime { get; set; }
    
    public long SelectionEndTime { get; set; }
    
    public int SelectionStartIndex { get; set; }
    
    public int SelectionEndIndex { get; set; }

    public List<int> SelectedIndices { get; } = new();
}