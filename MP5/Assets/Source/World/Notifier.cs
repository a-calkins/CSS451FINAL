using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notifier<T> : MonoBehaviour
{
    public event Action<T> NewValue;

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
