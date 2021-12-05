using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TransformNotifier))]
public class TransformControl : MonoBehaviour
{

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
        Vector3 p = ReadObjectTransfrom();
        previousSliderValues = p;
        X.InitSliderRange(-20, 20, p.x);
        Y.InitSliderRange(-20, 20, p.y);
        Z.InitSliderRange(-20, 20, p.z);
    }

    void SetToScaling(bool v)
    {
        Vector3 s = ReadObjectTransfrom();
        previousSliderValues = s;
        X.InitSliderRange(0.1f, 20, s.x);
        Y.InitSliderRange(0.1f, 20, s.y);
        Z.InitSliderRange(0.1f, 20, s.z);
    }

    void SetToRotation(bool v)
    {
        Vector3 r = ReadObjectTransfrom();
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
        UpdateCache(0, v);
        Vector3 p = ReadObjectTransfrom();
        // if not in rotation, next two lines of work would be wasted
        float dx = v - previousSliderValues.x;
        previousSliderValues.x = v;
        Quaternion q = Quaternion.AngleAxis(dx, Vector3.right);
        p.x = v;
        SetObjectTransform(ref p, ref q);
    }

    void YValueChanged(float v)
    {
        UpdateCache(1, v);
        Vector3 p = ReadObjectTransfrom();
        // if not in rotation, next two lines of work would be wasted
        float dy = v - previousSliderValues.y;
        previousSliderValues.y = v;
        Quaternion q = Quaternion.AngleAxis(dy, Vector3.up);
        p.y = v;
        SetObjectTransform(ref p, ref q);
    }

    void ZValueChanged(float v)
    {
        if (!R.isOn)
        {
            return;
        }
        UpdateCache(2, v);
        Vector3 p = ReadObjectTransfrom();
        // if not in rotation, next two lines of work would be wasterd
        float dz = v - previousSliderValues.z;
        previousSliderValues.z = v;
        Quaternion q = Quaternion.AngleAxis(dz, Vector3.forward);
        p.z = v;
        SetObjectTransform(ref p, ref q);
    }

    void UpdateCache(int axis, float v)
    {
        if (T.isOn)
        {
            translation[axis] = v;
        } else if (R.isOn)
        {
            rotation[axis] = v;
        } else if (S.isOn)
        {
            scale[axis] = v;
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

    private Vector3 ReadObjectTransfrom()
    {
        Vector3 p;

        if (T.isOn)
        {
            if (selected != null)
                p = selected.localPosition;
            else
                p = Vector3.zero;
        }
        else if (S.isOn)
        {
            if (selected != null)
                p = selected.localScale;
            else
                p = Vector3.one;
        }
        else
        {
            p = Vector3.zero;
        }
        return p;
    }

    private void SetObjectTransform(ref Vector3 p, ref Quaternion q)
    {
        /*
        if (selected == null)
            return;

        if (T.isOn)
        {
            selected.localPosition = p;
        }
        else if (S.isOn)
        {
            selected.localScale = p;
        }
        else
        {
            selected.localRotation *= q;
        }
        */

        if (notifier != null)
        {
            notifier.UpdateValue(new TransformNotifier.Transform(p, q));
        }
    }
}
