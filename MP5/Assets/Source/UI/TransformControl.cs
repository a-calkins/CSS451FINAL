using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TransformNotifier))]
public class TransformControl : MonoBehaviour
{
    private enum Axis
    {
        X,
        Y,
        Z
    }

    public Toggle T, R, S;
    public SliderWithEcho X, Y, Z;
    public Text ObjectName;

    private Transform selected;
    private Vector3 previousSliderValues = Vector3.zero;

    private Vector3 translation, rotation, scale;

    private TransformNotifier notifier;

    // Use this for initialization
    void Start()
    {
        T.onValueChanged.AddListener(delegate(bool on) { if (on) ObjectSetUI(); });
        R.onValueChanged.AddListener(delegate (bool on) { if (on) ObjectSetUI(); });
        S.onValueChanged.AddListener(delegate (bool on) { if (on) ObjectSetUI(); });
        X.SetSliderListener(XValueChanged);
        Y.SetSliderListener(YValueChanged);
        Z.SetSliderListener(ZValueChanged);

        T.isOn = true;
        R.isOn = false;
        S.isOn = false;
        notifier = GetComponent<TransformNotifier>();
        SetToTranslation(true);
    }

    //---------------------------------------------------------------------------------
    // Initialize slider bars to specific function
    void SetToTranslation(bool v)
    {
        Vector3 p = ReadObjectTransform();
        previousSliderValues = p;
        X.InitSliderRange(-20, 20, p.x);
        Y.InitSliderRange(-20, 20, p.y);
        Z.InitSliderRange(-20, 20, p.z);
    }

    void SetToScaling(bool v)
    {
        Vector3 s = ReadObjectTransform();
        previousSliderValues = s;
        X.InitSliderRange(0.1f, 20, s.x);
        Y.InitSliderRange(0.1f, 20, s.y);
        Z.InitSliderRange(0.1f, 20, s.z);
    }

    void SetToRotation(bool v)
    {
        Vector3 r = ReadObjectTransform();
        previousSliderValues = r;
        X.InitSliderRange(-180, 180, r.x);
        Y.InitSliderRange(-180, 180, r.y);
        Z.InitSliderRange(-180, 180, r.z);
        previousSliderValues = r;
    }
    //---------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------
    // resopond to slider bar value changes
    void XValueChanged(float v)
    {
        UpdateCache(Axis.X, v);
        SetObjectTransform(ReadObjectTransform());
    }

    void YValueChanged(float v)
    {
        UpdateCache(Axis.Y, v);
        SetObjectTransform(ReadObjectTransform());
    }

    void ZValueChanged(float v)
    {
        if (!R.isOn)
        {
            return;
        }
        UpdateCache(Axis.Z, v);
        SetObjectTransform(ReadObjectTransform());
    }

    void UpdateCache(Axis axis, float v)
    {
        if (T.isOn)
        {
            translation[(int)axis] = v;
        } else if (R.isOn)
        {
            rotation[(int)axis] = v;
        } else if (S.isOn)
        {
            scale[(int)axis] = v;
        }
    }
    //---------------------------------------------------------------------------------

    // new object selected
    public void SetSelectedObject(Transform transform)
    {
        selected = transform;
        previousSliderValues = Vector3.zero;
        if (transform != null)
            ObjectName.text = "Selected:" + transform.name;
        else
            ObjectName.text = "Selected: none";
        ObjectSetUI();
    }

    public void ObjectSetUI()
    {
        Vector3 p = Vector3.zero;
        if (T.isOn)
        {
            p = translation;
        } else if (R.isOn)
        {
            p = rotation;
        } else if (S.isOn)
        {
            p = scale;
        }
        X.SetSliderValue(p.x);  // do not need to call back for this comes from the object
        Y.SetSliderValue(p.y);
        Z.SetSliderValue(p.z);
    }

    private Vector3 ReadObjectTransform()
    {
        Vector3 p = Vector3.zero;
        if (T.isOn)
        {
            p = translation;
        }
        else if (R.isOn)
        {
            p = rotation;
        }
        else if (S.isOn)
        {
            p = scale;
        }
        return p;
    }

    private void SetObjectTransform(Vector3 p)
    {
        if (notifier != null)
        {
            notifier.UpdateValue(new TransformNotifier.Transform(translation, rotation, scale));
        }
    }
}
