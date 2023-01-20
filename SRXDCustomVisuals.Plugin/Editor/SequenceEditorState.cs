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

    public List<int>[] SelectedIndicesPerColumn { get; }
    
    public bool ShowValue { get; set; }

    public SequenceEditorState() {
        SelectedIndicesPerColumn = new List<int>[256];

        for (int i = 0; i < 256; i++)
            SelectedIndicesPerColumn[i] = new List<int>();
    }
}