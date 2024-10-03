using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class Heap<T> where T : IHeapItem<T>
{
    public readonly int length;
    private readonly T[] items;
    private int currentItemCount;
    private readonly T[] children = new T[2];

    public int Count { get { return currentItemCount; } }

    public Heap(int mapHeapSize)
    {
        length = mapHeapSize;
        items = new T[length];
    }

    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    public T RemoveFirst()
    {
        T firstItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    public T First()
    {
        return items[0];
    }

    public T[] GetChildren(T item)
    {
        int childIndexLeft = item.HeapIndex * 2 + 1;
        int childIndexRight = item.HeapIndex * 2 + 2;

        children[0] = items[childIndexLeft];
        children[1] = items[childIndexRight];

        return children;
    }
    public T[] GetChildrenOnIndex(int index)
    {
        int childIndexLeft = index * 2 + 1;
        int childIndexRight = index * 2 + 2;

        if (childIndexLeft < length)
        {
            children[0] = items[childIndexLeft];
        }
        else
        {
            children[0] = default;
        }

        if (childIndexRight < length)
        {
            children[1] = items[childIndexRight];
        }
        else
        {
            children[1] = default;
        }

        return children;
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    public void Clear()
    {
        for (int i = currentItemCount; i >= 0; i--)
        {
            items[i] = default;
        }

        currentItemCount = 0;
    }

    void SortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < currentItemCount)
            {
                swapIndex = childIndexLeft;

                if (childIndexRight < currentItemCount)
                {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                if (item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(item, items[swapIndex]);
                }
                else
                {
                    return;
                }

            }
            else
            {
                return;
            }

        }
    }

    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    private void Swap(T itemA, T itemB)
    {
        (items[itemA.HeapIndex], items[itemB.HeapIndex]) = (items[itemB.HeapIndex], items[itemA.HeapIndex]);

        (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex { get; set; }
}
