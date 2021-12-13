using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class NodePrimitive: MonoBehaviour {
    public Color MyColor = new Color(0.1f, 0.1f, 0.2f, 1.0f);
    private Color displayColor;
    public Vector3 Pivot;

    // Rotate
    public Vector3 RotateAxis = Vector3.up;
    public float RotateRange = 120f;
    public float AngularSpeed = 20f;  // Degree per second
    public bool Rotate = false;

    private int mRotateDirection = 1;
    private float mCurrentAngle = 0f;

    private int colorStepsLeft = 0;
    private static readonly int COLOR_STEPS = 50;

    private new Renderer renderer;

	// Use this for initialization
	void Start () {
        renderer = GetComponent<Renderer>();
        GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f));
    }

    void Update()
    {
        if (!Rotate)
            return;

        if (Mathf.Abs(mCurrentAngle) > RotateRange)
        {
            mRotateDirection *= -1;
        }
        float delta = AngularSpeed * Time.fixedDeltaTime * mRotateDirection;
        mCurrentAngle += delta;
        Quaternion q = Quaternion.AngleAxis(delta, RotateAxis);
        transform.localRotation = q * transform.localRotation;
    }

    public void SetGrayscale(bool condition)
    {
        if (condition)
        {
            displayColor = MyColor;
        } else
        {
            displayColor = new Color(0.6f, 0.6f, 0.6f);
        }
    }

    public void FlashBlack()
    {
        //displayColor = new Color(0, 0, 0);
        colorStepsLeft = COLOR_STEPS;
    }

    public void LoadShaderMatrix(ref Matrix4x4 nodeMatrix)
    {
        Matrix4x4 p = Matrix4x4.TRS(Pivot, Quaternion.identity, Vector3.one);
        Matrix4x4 invP = Matrix4x4.TRS(-Pivot, Quaternion.identity, Vector3.one);
        Matrix4x4 trs = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        Matrix4x4 m = nodeMatrix * p * trs * invP;
        renderer.material.SetMatrix("MyTRSMatrix", m);

        if (colorStepsLeft > 0)
        {
            renderer.material.SetColor("MyColor", new Color(
                MyColor.r * (COLOR_STEPS - colorStepsLeft) / COLOR_STEPS,
                MyColor.g * (COLOR_STEPS - colorStepsLeft) / COLOR_STEPS,
                MyColor.b * (COLOR_STEPS - colorStepsLeft) / COLOR_STEPS
            ));
            colorStepsLeft--;
        }
        if (colorStepsLeft == 0) renderer.material.SetColor("MyColor", displayColor);
    }
}