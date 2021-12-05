using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TransformNotifier))]
[RequireComponent(typeof(Renderer))]
public class Controller : MonoBehaviour
{
    public TransformNotifier notifier;  // let other objects subscribe to this controller's position updates
    private Renderer normal;
    public Vector3 Normal { get { return normal.transform.up;  } }
    private Transform axes;

    private new Renderer renderer;

    void Awake()
    {
        notifier = GetComponent<TransformNotifier>();
        renderer = GetComponent<Renderer>();

        normal = transform.Find("normal").GetComponent<Renderer>();
        axes = transform.Find("axes").GetComponent<Transform>();
    }

    public Controller MoveSilentlyTo(Vector3 position)
    {
        transform.localPosition = position;
        return this;
    }

    public Controller MoveBy(Vector3 delta)
    {
        transform.localPosition += delta;
        notifier.UpdateValue(new TransformNotifier.Transform(delta));
        return this;
    }

    public Controller MoveSilentlyBy(Vector3 delta)
    {
        transform.localPosition += delta;
        notifier.UpdateValueSilently(new TransformNotifier.Transform(delta));
        return this;
    }

    public Controller SetRotation(Vector3 n)
    {
        // align the right axis with the normal
        if (Vector3.Dot(Vector3.right, n) + 1 < Mathf.Epsilon)
        {
            // this is a special case that breaks the FromToRotation below
            axes.localRotation = Quaternion.AngleAxis(180, Vector3.up);
        }
        else
        {
            axes.localRotation = Quaternion.FromToRotation(
                -Vector3.forward,
                Vector3.Cross(Vector3.up, n)
            );
        }
        SetNormal(n);
        return this;
    }

    public Controller SetNormal(Vector3 n)
    {
        normal.transform.up = n;
        normal.transform.localPosition = n;
        return this;
    }

    public Controller Hide()
    {
        renderer.enabled = false;
        normal.enabled = false;
        HideAxes();
        return this;
    }

    public Controller Show()
    {
        renderer.enabled = true;
        normal.enabled = true;
        return this;
    }

    public Controller ShowIf(bool visible)
    {
        if (visible)
        {
            Show();
        } else
        {
            Hide();
        }
        return this;
    }

    public Controller HideAxes()
    {
        foreach (Transform axis in axes)
        {
            axis.GetComponent<Renderer>().enabled = false;
        }
        return this;
    }

    public Controller ShowAxes()
    {
        foreach (Transform axis in axes)
        {
            axis.GetComponent<Renderer>().enabled = true;
        }
        return this;
    }

    public Controller Select()
    {
        renderer.material.SetColor("_Color", Color.black);
        ShowAxes();
        return this;
    }

    public Controller Deselect()
    {
        renderer.material.SetColor("_Color", Color.white);
        HideAxes();
        return this;
    }
}
