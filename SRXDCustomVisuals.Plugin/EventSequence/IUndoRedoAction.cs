namespace SRXDCustomVisuals.Plugin; 

public interface IUndoRedoAction {
    void Undo();

    void Redo();
}