using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cylinder : MeshPresets.Generator
{
    public static MeshPresets.SliderValues Resolution = new MeshPresets.SliderValues(4, 20, 10);
    public static MeshPresets.SliderValues Size = new MeshPresets.SliderValues(10, 360, 180);

    private int HEIGHT = 10;
    private int RADIUS = 3;
    private float circleConstant;

    public Cylinder(int numVertices, int size)
    {
        this.numVertices = numVertices;
        this.size = size;
        circleConstant = 2 * Mathf.PI / numQuads;
    }

    public override Vector3 Vertex(int x, int y)
    {
        return new Vector3(
            RADIUS * Mathf.Cos(circleConstant * x * size / (float)numQuads),
            y * (HEIGHT / (float)numVertices),
            RADIUS * Mathf.Sin(circleConstant * x * size / (float)numQuads)
        );
    }

    public override Vector3 Normal(int x, int y)
    {
        return Vector3.up;
    }
}
