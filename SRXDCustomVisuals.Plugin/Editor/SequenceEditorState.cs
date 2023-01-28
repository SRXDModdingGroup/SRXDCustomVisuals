using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class SequenceEditorState {
    public SequenceEditorMode Mode { get; set; }
    
    public long Time { get; set; }
    
    public int CursorIndex { get; set; }
    
    public int ColumnPan { get; set; }
    
    public bool Selecting { get; set; }
    
    public long SelectionStartTime { get; set; }
    
    public long SelectionEndTime { get; set; }
    
    public int SelectionStartIndex { get; set; }
    
    public int SelectionEndIndex { get; set; }

    public List<int>[] SelectedIndicesPerColumn { get; }
    
    public bool ShowValues { get; set; }
    
    public string BackgroundField { get; set; }

    public SequenceEditorState() {
        SelectedIndicesPerColumn = new List<int>[Constants.IndexCount];

        for (int i = 0; i < SelectedIndicesPerColumn.Length; i++)
            SelectedIndicesPerColumn[i] = new List<int>();
    }
}