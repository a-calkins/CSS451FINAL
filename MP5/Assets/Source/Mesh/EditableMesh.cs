using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public bool visible;

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
        UpdateControllers(vertices);
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
    public void UpdateControllers(Vector3[] vertices) {
        GameObject[] newControllers = new GameObject[vertices.Length];

        // if we already had some controllers, update them to new positions
        for (int i = 0; i < Mathf.Min(controllers.Length, newControllers.Length); i++)
        {
            newControllers[i] = controllers[i];
            newControllers[i].transform.GetComponent<MeshRenderer>().enabled = visible;
            for (int j = 0; j < 3; j++)
            {
                newControllers[i].transform.GetChild(j).GetComponent<Renderer>().enabled = false;
            }
            newControllers[i].transform.localPosition = vertices[i];
        }

        // fill in any controllers we still need to create
        for (int i = controllers.Length; i < newControllers.Length; i++) {
            newControllers[i] = Instantiate(Resources.Load("Prefabs/Controller") as GameObject);
            newControllers[i].transform.localPosition = vertices[i];
            newControllers[i].transform.parent = this.transform;

            newControllers[i].transform.GetComponent<MeshRenderer>().enabled = visible;
            for(int j = 0; j < 3; j++) {
                newControllers[i].transform.GetChild(j).GetComponent<Renderer>().enabled = false;
            }
            visible = false;
        }

        // yeet any surplus controllers (rip)
        for (int i = newControllers.Length; i < controllers.Length; i++)
        {
            Destroy(controllers[i]);
        }

        controllers = newControllers;
    }

    //make the controllers visible and interactable
    public void ShowControllers() {
        if(controllers != null && !visible) {
            for(int i = 0; i < controllers.Length; i++) {
                controllers[i].transform.GetComponent<MeshRenderer>().enabled = true;
            }
            visible = true;
        }
    }

    //hide the controllers and make them uninteractable
    public void HideControllers() {
        if(controllers != null && visible) {
            for(int i = 0; i < controllers.Length; i++) {
                controllers[i].transform.GetComponent<MeshRenderer>().enabled = false;
                for(int j = 0; j < 3; j++) {
                    controllers[i].transform.GetChild(j).GetComponent<Renderer>().enabled = false;
                }
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
