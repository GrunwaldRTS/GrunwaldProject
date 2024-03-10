using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Heap<T> where T : IHeapElement<T>
{
    T[] elements;
    int currentElementsCount;
    public Heap(int maxHeapSize)
    {
        elements = new T[maxHeapSize];
    }
    public void Add(T element)
    {
        elements[currentElementsCount] = element;
        element.HeapIndex = currentElementsCount;
        currentElementsCount++;
        SortUp(element);
    }
    public T RemoveFirst()
    {
        T firstElement = elements[0];
        currentElementsCount--;
        elements[0] = elements[currentElementsCount];
        elements[0].HeapIndex = 0;
        SortDown(elements[0]);

        return firstElement;
    }
    void SortDown(T element)
    {
        while (true)
        {
			int leftChildIndex = 2 * element.HeapIndex + 1;
			int rightChildIndex = 2 * element.HeapIndex + 2;
			int swapIndex = 0;

			if (leftChildIndex < currentElementsCount)
            {
                swapIndex = leftChildIndex;

                if (rightChildIndex < currentElementsCount && elements[leftChildIndex].CompareTo(elements[rightChildIndex]) < 0)
                {
                    swapIndex = rightChildIndex;
                }

                if (element.CompareTo(elements[swapIndex]) < 0)
                {
                    Swap(element, elements[swapIndex]);
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
    void SortUp(T element)
    {
        int parentIndex = (element.HeapIndex - 1) / 2;

        while (true)
        {
            T parentElement = elements[parentIndex];
            if (element.CompareTo(parentElement) > 0)
            {
                Swap(element, parentElement);
            }
            else
            {
                break;
            }

            parentIndex = (element.HeapIndex - 1) / 2;
        }
    }
    void Swap(T element1, T element2)
    {
        elements[element1.HeapIndex] = element2;
        elements[element2.HeapIndex] = element1;
        int ele1Index = element1.HeapIndex;
        element1.HeapIndex = element2.HeapIndex;
        element2.HeapIndex = ele1Index;
    }
    public int Count { get => currentElementsCount; }
    public bool Contains(T heapElement) 
    { 
        if(heapElement.HeapIndex < currentElementsCount)
        {
            return Equals(elements[heapElement.HeapIndex], heapElement);
		}
        else
        {
            return false;
        }
    }
    public void Clear() { currentElementsCount = 0; }
}
public interface IHeapElement<T> : IComparable<T>
{
    public int HeapIndex { get; set; }
}
