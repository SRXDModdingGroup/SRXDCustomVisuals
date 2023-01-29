using System;

namespace SRXDCustomVisuals.Plugin; 

public class UndoRedoAction : IUndoRedoAction {
    private Action undo;

    private Action redo;

    public UndoRedoAction(Action undo, Action redo) {
        this.undo = undo;
        this.redo = redo;
    }

    public void Undo() => undo();

    public void Redo() => redo();
}