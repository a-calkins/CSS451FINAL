using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMeshGenerator
{
    private static int HEIGHT = 10;
    private static int RADIUS = 1;

    public class Mesh
    {
        public readonly Vector3[] vertices;
        public readonly int[] triangles;
        public readonly Vector3[] normals;

        // for debugging
        public static readonly Mesh EMPTY = new Mesh(new Vector3[] { }, new int[] { }, new Vector3[] { });

        public Mesh(Vector3[] v, int[] t, Vector3[] n)
        {
            vertices = v;
            triangles = t;
            normals = n;
        }
    }

    public static Mesh Generate(string name, int resolution, int length)
    {
        return name switch
        {
            "mesh" => Plane(resolution, length),
            "cylinder" => Cylinder(resolution, length),
            _ => Mesh.EMPTY
        };
    }

    public static Mesh Plane(int numVertices, int length)
    {
        int numQuads = numVertices - 1;

        // todo: refactor into another method that just takes an equation
        Vector3[] v = new Vector3[numVertices * numVertices];
        for (int i = 0; i < numVertices; i++)
        {
            for (int j = 0; j < numVertices; j++)
            {
                v[i * numVertices + j] = new Vector3(
                    j * length / (float)numQuads,
                    0,
                    i * length / (float)numQuads
                );
            }
        }

        // todo: refactor into another method that just follows the same pattern for all inputs
        int[] t = new int[(numQuads * numQuads) * 2 * 3];
        for (int tIndex = 0; tIndex < (numQuads * numQuads) * 6; tIndex += 6)
        {
            int vIndex = tIndex / 6;
            vIndex += vIndex / numQuads;  // if we're at an edge, roll over to the next row

            t[tIndex + 2] = vIndex;
            t[tIndex + 1] = vIndex + 1;
            t[tIndex + 0] = vIndex + numVertices;

            t[tIndex + 3] = vIndex + 1;
            t[tIndex + 4] = vIndex + numVertices;
            t[tIndex + 5] = vIndex + 1 + numVertices;
        }

        // todo: refactor into another method that takes an equation
        Vector3[] n = new Vector3[numVertices * numVertices];
        for (int i = 0; i < numVertices * numVertices; i++)
        {
            n[i] = Vector3.up;
        }

        return new Mesh(v, t, n);
    }

    public static Mesh Cylinder(int numVertices, int length)
    {
        int numQuads = numVertices - 1;
        float cylinderConstant = 2 * Mathf.PI / numQuads;

        // todo: refactor into another method that just takes an equation
        Vector3[] v = new Vector3[numVertices * numVertices];
        for (int i = 0; i < numVertices; i++)
        {
            for (int j = 0; j < numVertices; j++)
            {
                v[i * numVertices + j] = new Vector3(
                    RADIUS * Mathf.Cos(cylinderConstant * j * length / (float)numQuads),
                    i * (HEIGHT / (float)numVertices),
                    RADIUS * Mathf.Sin(cylinderConstant * j * length / (float)numQuads)
                );
            }
        }

        // todo: refactor into another method that just follows the same pattern for all inputs
        int[] t = new int[(numQuads * numQuads) * 2 * 3 * 2];
        for (int tIndex = 0; tIndex < (numQuads * numQuads) * 6; tIndex += 6)
        {
            int vIndex = tIndex / 6;
            vIndex += vIndex / numQuads;

            t[tIndex + 2] = vIndex;
            t[tIndex + 1] = vIndex + 1;
            t[tIndex + 0] = vIndex + numVertices;

            t[tIndex + 0 + numQuads] = vIndex;
            t[tIndex + 1 + numQuads] = vIndex + 1;
            t[tIndex + 2 + numQuads] = vIndex + numVertices;

            t[tIndex + 3] = vIndex + 1;
            t[tIndex + 4] = vIndex + numVertices;
            t[tIndex + 5] = vIndex + 1 + numVertices;

            t[tIndex + 5 + numQuads] = vIndex + 1;
            t[tIndex + 4 + numQuads] = vIndex + numVertices;
            t[tIndex + 3 + numQuads] = vIndex + 1 + numVertices;
        }

        // todo: refactor into another method that takes an equation
        Vector3[] n = new Vector3[numVertices * numVertices];
        for (int i = 0; i < numVertices * numVertices; i++)
        {
            n[i] = (new Vector3(0, i / numVertices, 0) - v[i]).normalized;
        }

        return new Mesh(v, t, n);
    }
}
