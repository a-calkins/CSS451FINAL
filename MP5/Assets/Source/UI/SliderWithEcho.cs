using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FloatNotifier))]
public class SliderWithEcho : MonoBehaviour
{
    public float min = 1;
    public float max = 10;
    public float initialValue = 5;
    public bool truncate = true;

    public Slider slider = null;
    public Text echo = null;
    public Text label = null;

    // todo: replace these with Notifiers i guess
    public delegate void SliderCallbackDelegate(float v);
    private SliderCallbackDelegate callback = null;

    private FloatNotifier notifier;


    // Use this for initialization
    void Start()
    {
        Debug.Assert(slider != null);
        Debug.Assert(echo != null);
        Debug.Assert(label != null);

        notifier = GetComponent<FloatNotifier>();
        InitSliderRange(min, max, initialValue);
        slider.onValueChanged.AddListener(SliderValueChange);
    }

    public void SetSliderListener(SliderCallbackDelegate listener)
    {
        callback = listener;
    }

    // GUI element changes the object
    void SliderValueChange(float v)
    {
        echo.text = v.ToString(truncate ? "0.0" : "0.0000");
        // Debug.Log("SliderValueChange: " + v);
        if (callback != null)
            callback(v);
        notifier.UpdateValue(v);
    }

    public float GetSliderValue()
    {
        return slider.value;
    }
    public void SetSliderLabel(string l)
    {
        label.text = l;
    }
    public void SetSliderValue(float v)
    {
        slider.value = v; SliderValueChange(v);
    }

    public void InitSliderRange(float min, float max, float v)
    {
        slider.minValue = min;
        slider.maxValue = max;
        SetSliderValue(v);
    }
}
