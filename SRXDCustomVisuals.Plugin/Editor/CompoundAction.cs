using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class CompoundAction : IUndoRedoAction {
    public int Count => actions.Count;
    
    private List<IUndoRedoAction> actions = new();

    public void AddAction(IUndoRedoAction action) => actions.Add(action);
    
    public void Undo() {
        for (int i = actions.Count - 1; i >= 0; i--)
            actions[i].Undo();
    }

    public void Redo() {
        foreach (var action in actions)
            action.Redo();
    }
}