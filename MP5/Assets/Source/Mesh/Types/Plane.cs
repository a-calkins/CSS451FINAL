using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MeshTypes.Generator
{
    public static SliderWithEcho.Values Resolution = new SliderWithEcho.Values(2, 20, 10);
    public static SliderWithEcho.Values Size = new SliderWithEcho.Values(1, 10, 5);

    public Plane(int numVertices, int size)
    {
        this.numVertices = numVertices;
        this.size = size;
    }

    public override Vector3 Vertex(int x, int y)
    {
        return new Vector3(
            x * size / (float)numQuads,
            0,
            y * size / (float)numQuads
        );
    }

    public override Vector3 Normal(int x, int y)
    {
        return Vector3.up;
    }
}
