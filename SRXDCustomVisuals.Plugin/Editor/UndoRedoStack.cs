using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class UndoRedoStack {
    public bool CanUndo => currentIndex >= 0 && currentIndex < actions.Count;

    public bool CanRedo => currentIndex >= -1 && currentIndex < actions.Count - 1;
    
    private List<IUndoRedoAction> actions = new();
    private int currentIndex = -1;

    public void AddAction(IUndoRedoAction action) {
        for (int i = actions.Count - 1; i > currentIndex; i--)
            actions.RemoveAt(i);
        
        actions.Add(action);
        currentIndex++;
    }

    public void Undo() {
        if (!CanUndo)
            return;
        
        actions[currentIndex].Undo();
        currentIndex--;
    }

    public void Redo() {
        if (!CanRedo)
            return;
        
        currentIndex++;
        actions[currentIndex].Redo();
    }
}