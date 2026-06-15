using System;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<TElement, TPriority> where TPriority : struct, IComparable<TPriority>
{
    private (TElement element, TPriority priority)[] heap;
    private const int initialHeapSize = 50;
    private int head; // 현재 사용되는 힙의 최소 인덱스
    private int tail; // 현재 사용되는 힙의 최대 인덱스 + 1
    private readonly Dictionary<TElement, int> indexMap = new();

    public int Count => tail - head;
    public int Capacity => heap.Length;

    public PriorityQueue()
    {
        heap = new (TElement, TPriority)[initialHeapSize];
    }
    
    public void Clear()
    {        
        indexMap.Clear();
        head = 0;
        tail = 0;
    }

    public void Enqueue(TElement element, TPriority priority)
    {
        if (indexMap.ContainsKey(element))
        {
            UpdateKey(element, priority);
            return;
        }

        heap[tail] = (element, priority);
        indexMap.Add(element, tail);
        ShiftUp(tail);
        tail++;

        if (tail >= heap.Length)
        {
            EnlargeHeap();
        }
    }

    public TElement Dequeue()
    {
        TElement result = heap[head++].element;

        indexMap.Remove(result);

        ShiftDown(head);
        if (head > heap.Length / 2)
        {
            AdhereHeapToLeft();
        }

        return result;
    }

    private void UpdateKey(TElement element, TPriority priority)
    {
        int compareResult = ComparePriority(priority, heap[indexMap[element]].priority);

        if (compareResult < 0)
        {
            DecreaseKey(element, priority);
        }
        else if (compareResult > 0)
        {
            IncreaseKey(element, priority);
        }
    }

    private void IncreaseKey(TElement element, TPriority biggerPriority)
    {
        int index = indexMap[element];
        heap[index] = (element, biggerPriority);
        if (index < head)
        {
            Swap(index, --head);
            ShiftDown(head);
        }
        else
            ShiftDown(index);
    }

    private void DecreaseKey(TElement element, TPriority smallerPriority)
    {
        int index = indexMap[element];
        heap[index] = (element, smallerPriority);
        ShiftUp(index);
    }

    private void ShiftUp(int index)
    {
        int parentIndex = (index - head - 1) / 2 + head;
        if (index - head <= 0) return;

        if (ComparePriority(heap[index].priority, heap[parentIndex].priority) < 0)
        {
            Swap(index, parentIndex);
            ShiftUp(parentIndex);
        }
    }

    private void ShiftDown(int i)
    {
        int smallest = i;
        int left = (i - head) * 2 + 1 + head;
        int right = (i - head) * 2 + 2 + head;

        TPriority prioritySmallest = heap[smallest].priority;
        TPriority priorityChild;
        if (left < tail)
        {
            try
            {
                priorityChild = heap[left].priority;
            }
            catch
            {
                Debug.Log($"{left} {i} {head} {tail} {heap.Length}");
                return;
            }
            if (ComparePriority(priorityChild, prioritySmallest) < 0)
            {
                smallest = left;
                prioritySmallest = heap[smallest].priority;
            }
        }

        if (right < tail)
        {
            priorityChild = heap[right].priority;
            if (ComparePriority(priorityChild, prioritySmallest) < 0)
            {
                smallest = right;
            }
        }

        if (smallest != i)
        {
            Swap(i, smallest);
            ShiftDown(smallest);
        }
    }

    private int ComparePriority(TPriority a, TPriority b) => Comparer<TPriority>.Default.Compare(a, b);

    private void Swap(int a, int b) => (heap[b], heap[a]) = (heap[a], heap[b]);

    private void AdhereHeapToLeft()
    {
        int count = 0;
        for (int i = head; i < tail; i++)
        {
            heap[count++] = heap[i];
        }
        tail -= head;
        head = 0;
    }

    private void EnlargeHeap()
    {
        (TElement, TPriority)[] newHeap = new (TElement, TPriority)[heap.Length + initialHeapSize];
        Array.Copy(heap, head, newHeap, 0, tail - head);
        heap = newHeap;
        tail -= head;
        head = 0;
    }
}
