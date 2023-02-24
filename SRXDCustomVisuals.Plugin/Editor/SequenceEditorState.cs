using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class SequenceEditorState {
    public SequenceEditorMode Mode { get; set; }
    
    public long Time { get; set; }
    
    public int Column { get; set; }
    
    public int ColumnPan { get; set; }
    
    public bool Selecting { get; set; }
    
    public long SelectionStartTime { get; set; }
    
    public long SelectionEndTime { get; set; }
    
    public int SelectionStartIndex { get; set; }
    
    public int SelectionEndIndex { get; set; }

    public List<int>[] SelectedIndicesPerColumn { get; }
    
    public bool ShowValues { get; set; }
    
    public string BackgroundField { get; set; }
    
    public List<string[]> PaletteFields { get; }

    public SequenceEditorState() {
        SelectedIndicesPerColumn = new List<int>[Constants.IndexCount];

        for (int i = 0; i < SelectedIndicesPerColumn.Length; i++)
            SelectedIndicesPerColumn[i] = new List<int>();

        PaletteFields = new List<string[]>(Constants.PaletteSize);

        for (int i = 0; i < Constants.PaletteSize; i++) {
            string[] fields = new string[3];
            
            for (int j = 0; j < 3; j++)
                fields[j] = string.Empty;
            
            PaletteFields.Add(fields);
        }
    }
}