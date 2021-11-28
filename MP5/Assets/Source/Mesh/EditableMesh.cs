using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class EditableMesh : MonoBehaviour
{
    private MeshFilter meshFilter;

    // turns off half the faces to make the vertices easier to see
    public bool debugVertices;
    new public string name;

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        Debug.Assert(meshFilter != null);
    }

    public void UpdateMesh(Vector3[] vertices, int[] triangles, Vector3[] normals)
    {
        if (meshFilter == null)
        {
            // this literally shouldn't be possible idk why it's happening
            Debug.Log("MeshFilter was null");
            meshFilter = GetComponent<MeshFilter>();
        }
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
}
