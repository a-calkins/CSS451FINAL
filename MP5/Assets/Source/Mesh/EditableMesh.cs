using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class EditableMesh : MonoBehaviour
{
    // turns off half the faces to make the vertices easier to see
    public bool debugVertices;
    private MeshFilter meshFilter;
    private SliderWithEcho.Values resolution;
    private SliderWithEcho.Values size;
    new public string name;

    void Awake()
    {
        // if we do this in Start instead of Awake, meshFilter will be null
        // the first time UpdateMesh() is called from other classes' Start
        // methods
        meshFilter = GetComponent<MeshFilter>();
    }

    private void ChangeMesh(Vector3[] vertices, int[] triangles, Vector3[] normals)
    {
        meshFilter.mesh.Clear();
        meshFilter.mesh.SetVertices(vertices);
        if (debugVertices)
        {
            for (int i = 0; i < triangles.Length; i += 6)
            {
                // won't work because we have the no-cull shader
                // (triangles[i], triangles[i + 2]) = (triangles[i + 2], triangles[i]);
                triangles[i] = triangles[i + 1] = triangles[i + 2] = 0;
            }
        }
        meshFilter.mesh.SetTriangles(triangles, 0);
        meshFilter.mesh.SetNormals(normals);
    }

    public void ChangeMesh(MeshTypes.Mesh mesh)
    {
        ChangeMesh(mesh.vertices, mesh.triangles, mesh.normals);
    }

    public void ChangeSliders(SliderWithEcho.Values resolution, SliderWithEcho.Values size)
    {
        this.resolution = resolution;
        this.size = size;
    }

    public void Resolution(int val)
    {
        if (resolution != null)
        {
            resolution.value = val;
        }
    }

    public void Size(int val)
    {
        if (size != null)
        {
            size.value = val;
        }
    }
}
