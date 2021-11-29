using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPresets
{

    // wraps the arrays of vertices, triangles, and normals
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

    // lets individual mesh classes store their resolution & scale stuff statically
    public class SliderValues
    {
        public int min;
        public int max;
        public int value;

        public SliderValues(int min, int max, int value)
        {
            this.min = min;
            this.max = max;
            this.value = value;
        }
    }

    // individual mesh generators (plane, cylinder, possibly others for extra
    // credit?) inherit from this class
    // + re extra credit: i guess projecting 2D x & y onto 3D like this is a
    // lame/poor abstraction for shapes that aren't basically a distorted
    // plane, e.g. plane and cylinder
    // but who knows maybe we could find another one (torus could work?)
    public abstract class Generator
    {
        protected int numVertices;
        protected int numQuads { get { return numVertices - 1; } }
        protected int size;

        public static SliderValues sliderValues;

        public abstract Vector3 Vertex(int x, int y);
        public abstract Vector3 Normal(int x, int y);

        public int[] Triangles()
        {
            int[] t = new int[(numQuads * numQuads) * 2 * 3];
            for (int tIndex = 0; tIndex < (numQuads * numQuads) * 6; tIndex += 6)
            {
                int vIndex = tIndex / 6;
                // if we're at an edge, roll over to the next row
                // to avoid making a huge triangle that straddles rows
                vIndex += vIndex / numQuads;

                t[tIndex + 2] = vIndex;
                t[tIndex + 1] = vIndex + 1;
                t[tIndex + 0] = vIndex + numVertices;

                t[tIndex + 3] = vIndex + 1;
                t[tIndex + 4] = vIndex + numVertices;
                t[tIndex + 5] = vIndex + 1 + numVertices;
            }
            return t;
        }

        public Vector3[] Vertices()
        {
            Vector3[] v = new Vector3[numVertices * numVertices];
            for (int y = 0; y < numVertices; y++)
            {
                for (int x = 0; x < numVertices; x++)
                {
                    v[y * numVertices + x] = Vertex(x, y);
                }
            }
            return v;
        }

        public Vector3[] Normals()
        {
            Vector3[] n = new Vector3[numVertices * numVertices];
            for (int y = 0; y < numVertices; y++)
            {
                for (int x = 0; x < numVertices; x++)
                {
                    n[y * numVertices + x] = Normal(x, y);
                }
            }
            return n;
        }
    }

    public static SliderValues GetResolutionValues(string name)
    {
        return name switch
        {
            "mesh" => Plane.Resolution,
            "cylinder" => Cylinder.Resolution,
            _ => null
        };
    }

    public static SliderValues GetSizeValues(string name)
    {
        return name switch
        {
            "mesh" => Plane.Size,
            "cylinder" => Cylinder.Size,
            _ => null
        };
    }

    public static Mesh Generate(string name, int resolution, int length)
    {
        return name switch
        {
            "mesh" => Generate(new Plane(resolution, length)),
            "cylinder" => Generate(new Cylinder(resolution, length)),
            _ => Mesh.EMPTY
        };
    }

    private static Mesh Generate(Generator generator)
    {
        return new Mesh(generator.Vertices(), generator.Triangles(), generator.Normals());
    }
}
