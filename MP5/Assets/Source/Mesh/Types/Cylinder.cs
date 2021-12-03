using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cylinder : MeshTypes.Generator
{

    // we can modify these two
    private readonly int HEIGHT = 10;
    private readonly int RADIUS = 3;

    public static SliderWithEcho.Values Resolution = new SliderWithEcho.Values(4, 20, 10);
    public static SliderWithEcho.Values Size = new SliderWithEcho.Values(10, 360, 180);

    public Cylinder(int numVertices, int maxDegree)
    {
        this.numVertices = numVertices;
        this.size = maxDegree;
    }

    public override Vector3 Vertex(int x, int y)
    {
        return new Vector3(
            RADIUS * Mathf.Cos(Mathf.Deg2Rad * (x * size / (float)numQuads)),
            y * (HEIGHT / (float)numQuads) - HEIGHT / 2f,
            RADIUS * Mathf.Sin(Mathf.Deg2Rad * (x * size / (float)numQuads))
        );
    }

    public override Vector3 Normal(int x, int y)
    {
        // todo: do the actual math here instead of just calling Vertex()
        Vector3 vertex = Vertex(x, y);
        return (vertex - new Vector3(0, vertex.y, 0)).normalized;
    }
}