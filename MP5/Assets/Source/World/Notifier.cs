using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notifier<T> : MonoBehaviour
{
    public delegate void Notify(T val);
    public event Notify NewValue;

    public T current { get; private set; }

    public void UpdateValueSilently(T value)
    {
        current = value;
    }

    public void UpdateValue(T value)
    {
        UpdateValueSilently(value);
        if (NewValue != null)
        {
            NewValue(value);
        }
    }
}
