using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public interface IReadOnlySequenceElementCollection<out T> where T : ISequenceElement<T> {
    int ColumnCount { get; }

    int GetCountForColumn(int column);

    int GetFirstIndexAfterTime(int column, long time);

    int GetLastIndexBeforeTime(int column, long time);

    public T GetElement(int column, int index);

    public IReadOnlyList<T> GetElementsInColumn(int column);
}