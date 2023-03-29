using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class SequenceEditHandle<T> where T : ISequenceElement<T> {
    public IReadOnlySequenceElementCollection<T> Collection => collection;

    private SequenceElementCollection<T> collection;
    private CompoundAction compoundAction;

    public SequenceEditHandle(SequenceElementCollection<T> collection, CompoundAction compoundAction) {
        this.collection = collection;
        this.compoundAction = compoundAction;
    }
    
    public int AddElement(int column, T element) {
        int index = collection.AddElement(column, element);
        
        compoundAction.AddAction(new UndoRedoAction(
            () => collection.RemoveElement(column, index),
            () => collection.InsertElement(column, index, element)));

        return index;
    }

    public void AddElements(int column, IEnumerable<T> toAdd, List<int> indices) {
        var toAddSorted = new List<T>();

        foreach (var element in toAdd)
            toAddSorted.InsertSorted(element);

        indices.Clear();

        foreach (var element in toAddSorted)
            indices.InsertSorted(AddElement(column, element));
    }

    public void InsertElement(int column, int index, T element) {
        collection.InsertElement(column, index, element);
        
        compoundAction.AddAction(new UndoRedoAction(
            () => collection.RemoveElement(column, index),
            () => collection.InsertElement(column, index, element)));
    }

    public void RemoveElement(int column, int index) {
        var onOffEvent = collection.GetElement(column, index);
        
        collection.RemoveElement(column, index);
        compoundAction.AddAction(new UndoRedoAction(
            () => collection.InsertElement(column, index, onOffEvent),
            () => collection.RemoveElement(column, index)));
    }

    public void RemoveElements(int column, IEnumerable<int> indices) {
        var indicesSorted = new List<int>(indices);

        indicesSorted.Sort();

        for (int i = indicesSorted.Count - 1; i >= 0; i--)
            RemoveElement(column, indicesSorted[i]);
    }

    public void ReplaceElement(int column, int index, T element) {
        var oldElement = collection.GetElement(column, index);
        
        collection.ReplaceElement(column, index, element);
        compoundAction.AddAction(new UndoRedoAction(
            () => collection.ReplaceElement(column, index, oldElement),
            () => collection.ReplaceElement(column, index, element)));
    }
}