﻿using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class SequenceEditorState {
    public SequenceEditorMode Mode { get; set; }
    
    public long Time { get; set; }
    
    public int Column { get; set; }
    
    public int FirstColumnIndex { get; set; }
    
    public bool Selecting { get; set; }
    
    public long SelectionStartTime { get; set; }
    
    public long SelectionEndTime { get; set; }
    
    public int SelectionStartColumn { get; set; }
    
    public int SelectionEndColumn { get; set; }

    public List<int>[] SelectedIndicesPerColumn { get; }
    
    public bool ShowValues { get; set; }

    public TextFieldState BackgroundField { get; } = new();
    
    public List<TextFieldState> PaletteFields { get; }

    public SequenceEditorState() {
        SelectedIndicesPerColumn = new List<int>[Constants.IndexCount];

        for (int i = 0; i < SelectedIndicesPerColumn.Length; i++)
            SelectedIndicesPerColumn[i] = new List<int>();

        PaletteFields = new List<TextFieldState>(Constants.PaletteSize);

        for (int i = 0; i < Constants.PaletteSize; i++)
            PaletteFields.Add(new TextFieldState());
    }
}