using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TransformNotifier))]
[RequireComponent(typeof(Renderer))]
public class Controller : MonoBehaviour
{
    public EditableMesh parent;
    public TransformNotifier notifier;  // let other objects subscribe to this controller's position updates
    public Renderer normal;  // to be added

    private new Renderer renderer;

    void Awake()
    {
        notifier = GetComponent<TransformNotifier>();
        renderer = GetComponent<Renderer>();
    }

    public Controller MoveSilentlyTo(Vector3 position)
    {
        transform.localPosition = position;
        return this;
    }

    public Controller MoveBy(Vector3 delta)
    {
        transform.localPosition += delta;
        notifier.UpdateValue(new TransformNotifier.Transform(delta, Quaternion.identity));
        return this;
    }

    public Controller MoveSilentlyBy(Vector3 delta)
    {
        transform.localPosition += delta;
        notifier.UpdateValueSilently(new TransformNotifier.Transform(delta, Quaternion.identity));
        return this;
    }

    public Controller SetNormal(Vector3 n)
    {
        normal.transform.up = n;
        return this;
    }

    public Controller Hide()
    {
        renderer.enabled = false;
        if (normal != null)
        {
            normal.enabled = false;
        }
        return HideAxes();
    }

    public Controller Show()
    {
        renderer.enabled = true;
        if (normal != null)
        {
            normal.enabled = true;
        }
        return this;
    }

    public Controller ShowIf(bool visible)
    {
        if (visible)
        {
            return Show();
        }
        return Hide();
    }

    public Controller HideAxes()
    {
        foreach (Transform child in transform)
        {
            // skip the normal, only do the axes
            if (normal != null && ReferenceEquals(child.gameObject, normal.gameObject))
            {
                continue;
            }
            child.GetComponent<Renderer>().enabled = false;
        }
        return this;
    }

    public Controller ShowAxes()
    {
        foreach (Transform child in transform)
        {
            // skip the normal
            if (normal != null && ReferenceEquals(child.gameObject, normal.gameObject))
            {
                continue;
            }
            child.GetComponent<Renderer>().enabled = true;
        }
        return this;
    }

    public Controller Select()
    {
        renderer.material.SetColor("_Color", Color.black);
        return ShowAxes();
    }

    public Controller Deselect()
    {
        renderer.material.SetColor("_Color", Color.white);
        return HideAxes ();
    }
}
