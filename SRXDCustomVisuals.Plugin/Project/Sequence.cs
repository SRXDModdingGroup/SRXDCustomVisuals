using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Plugin; 

public class Sequence<T> : IReadOnlySequence<T> where T : ISequenceElement<T> {
    public int ColumnCount { get; }
    
    private List<T>[] columns;

    public Sequence(int columnCount) {
        ColumnCount = columnCount;
        columns = new List<T>[columnCount];

        for (int i = 0; i < columnCount; i++)
            columns[i] = new List<T>();
    }

    public int AddElement(int column, T element) {
        if (column < 0 || column >= ColumnCount)
            throw new ArgumentOutOfRangeException();
        
        return columns[column].InsertSorted(element);
    }

    public void InsertElement(int column, int index, T element) {
        if (column < 0 || column >= ColumnCount || index < 0)
            throw new ArgumentOutOfRangeException();

        var targetColumn = columns[column];
        
        if (index > targetColumn.Count)
            throw new ArgumentOutOfRangeException();

        if (index > 0 && element.Time < targetColumn[index - 1].Time || index < targetColumn.Count && element.Time > targetColumn[index].Time)
            throw new ArgumentException();
        
        targetColumn.Insert(index, element);
    }

    public void ReplaceElement(int column, int index, T element) {
        if (column < 0 || column >= ColumnCount || index < 0)
            throw new ArgumentOutOfRangeException();

        var targetColumn = columns[column];
        
        if (index >= targetColumn.Count)
            throw new ArgumentOutOfRangeException();

        if (element.Time != targetColumn[index].Time)
            throw new ArgumentException();

        targetColumn[index] = element;
    }

    public void RemoveElement(int column, int index) {
        if (column < 0 || column >= ColumnCount || index < 0)
            throw new ArgumentOutOfRangeException();

        var targetColumn = columns[column];
        
        if (index >= targetColumn.Count)
            throw new ArgumentOutOfRangeException();
        
        targetColumn.RemoveAt(index);
    }

    public int GetCountForColumn(int column) {
        if (column < 0 || column >= ColumnCount)
            throw new ArgumentOutOfRangeException();

        return columns[column].Count;
    }

    public int GetFirstIndexAfterTime(int column, long time) {
        if (column < 0 || column >= ColumnCount)
            throw new ArgumentOutOfRangeException();
        
        var targetColumn = columns[column];
        int index = DoBinarySearch();

        while (index < targetColumn.Count && time.CompareTo(targetColumn[index].Time) >= 0)
            index++;

        return index;

        int DoBinarySearch() {
            int start = 0;
            int end = targetColumn.Count - 1;

            while (start <= end) {
                int mid = (start + end) / 2;
                int comparison = time.CompareTo(targetColumn[mid].Time);

                if (comparison < 0)
                    end = mid - 1;
                else if (comparison > 0)
                    start = mid + 1;
                else
                    return mid;
            }

            return start;
        }
    }

    public int GetLastIndexBeforeTime(int column, long time) => GetFirstIndexAfterTime(column, time) - 1;

    public T GetElement(int column, int index) {
        if (column < 0 || column >= ColumnCount || index < 0)
            throw new ArgumentOutOfRangeException();

        var targetColumn = columns[column];
        
        if (index >= targetColumn.Count)
            throw new ArgumentOutOfRangeException();

        return targetColumn[index];
    }

    public IReadOnlyList<T> GetElementsInColumn(int column) {
        if (column < 0 || column >= ColumnCount)
            throw new ArgumentOutOfRangeException();

        return columns[column];
    }
}