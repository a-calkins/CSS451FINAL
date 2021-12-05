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

    // indicates that this mesh is symmetric on its X axis and that any changes
    // to controllers should be propagated along that axis
    // this should ideally be static but that would get sloppy so w/e
    // (TODO if we have time: allow either X or Y symmetry to be selected?)
    public override bool Symmetry {
        get { return true;  }
    }

    public Cylinder(int numVertices, int maxDegree)
    {
        this.numVertices = numVertices;
        size = maxDegree;
    }

    public override Vector3 Vertex(int x, int y)
    {
        return new Vector3(
            RADIUS * Mathf.Cos(Mathf.Deg2Rad * x * size / numQuads),
            y * (HEIGHT / (float)numQuads) - HEIGHT / 2f,
            RADIUS * Mathf.Sin(Mathf.Deg2Rad * x * size / numQuads)
        );
    }

    public override Vector3 Normal(int x, int y)
    {
        // todo: do the actual math here instead of just calling Vertex()
        Vector3 vertex = Vertex(x, y);
        return new Vector3(vertex.x, 0, vertex.z).normalized;
    }
}
