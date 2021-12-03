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
    //array to hold the controllers (spheres)
    public GameObject[] controllers;
    bool visible;
    void Awake()
    {
        // if we do this in Start instead of Awake, meshFilter will be null
        // the first time UpdateMesh() is called from other classes' Start
        // methods
        meshFilter = GetComponent<MeshFilter>();
    }

    private void SetMesh(Vector3[] vertices, int[] triangles, Vector3[] normals)
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
        InitControllers(vertices);
    }

    public void SetMesh(MeshTypes.Mesh mesh)
    {
        SetMesh(mesh.vertices, mesh.triangles, mesh.normals);
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
    //places the controllers on the mesh in the positions defined by its vertices.
    void InitControllers(Vector3[] vertices) {
        controllers = new GameObject[vertices.Length];
        for(int i = 0; i < vertices.Length; i++) {
            controllers[i] = GameObject.Instantiate(Resources.Load("Prefabs/Controller") as GameObject);

            controllers[i].transform.localPosition = vertices[i];
            controllers[i].transform.parent = this.transform;

            controllers[i].transform.GetComponent<MeshRenderer>().enabled = false;
            controllers[i].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            visible = false;
        }
    }
    //make the controllers visible and interactable
    public void ShowControllers() {
        if(controllers != null && !visible) {
            for(int i = 0; i < controllers.Length; i++) {
                controllers[i].transform.GetComponent<MeshRenderer>().enabled = true;
                controllers[i].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            }
            visible = true;
        }
    }
    //hide the controllers and make them uninteractable
    public void HideControllers() {
        if(controllers != null && visible) {
            for(int i = 0; i < controllers.Length; i++) {
                controllers[i].transform.GetComponent<MeshRenderer>().enabled = false;
                controllers[i].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            }
            visible = false;
        }
    }
    //updates the vertices when a controller is moved
    void Update() {
        Vector3[] v = meshFilter.mesh.vertices;
        if(controllers != null) {
            for(int i = 0; i < controllers.Length; i++) {
                v[i] = controllers[i].transform.localPosition;
            }
        }
        meshFilter.mesh.SetVertices(v);
    }
}
