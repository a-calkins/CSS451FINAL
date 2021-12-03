using System;
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
    public bool symmetry { get; private set; }

    //array to hold the controllers (spheres)
    private Controller[] controllers = new Controller[0];
    public bool visible;

    // array to hold functions that subscribe to controllers' position updates
    // so that we can delete them when we delete the controller
    private Action<TransformNotifier.Transform>[] delegatesToDelete = new Action<TransformNotifier.Transform>[0];

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
        UpdateControllers(vertices, normals);
    }

    public void SetMesh(MeshTypes.Mesh mesh)
    {
        symmetry = mesh.symmetry;
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
    public void UpdateControllers(Vector3[] vertices, Vector3[] normals) {
        Controller[] newControllers = new Controller[vertices.Length];
        var newDelegates = new Action<TransformNotifier.Transform>[vertices.Length];

        // if we already had some controllers, update them to new positions
        for (int i = 0; i < Mathf.Min(controllers.Length, newControllers.Length); i++)
        {
            newControllers[i] = controllers[i].ShowIf(visible).HideAxes().MoveSilentlyTo(vertices[i]);
            newDelegates[i] = delegatesToDelete[i];
        }

        // fill in any controllers we still need to create
        for (int i = controllers.Length; i < newControllers.Length; i++) {
            newControllers[i] = Instantiate(Resources.Load("Prefabs/Controller") as GameObject)
                .GetComponent<Controller>()
                .MoveSilentlyTo(vertices[i])
                .ShowIf(visible)
                .HideAxes();

            // attempt to get the normals in sync... doesn't quite work,
            // the Update() method is what actually takes care of the normals
            newControllers[i].transform.forward = normals[i];
            newControllers[i].transform.parent = transform;

            // we have to 'freeze 'these values because if we just pass (i, newControllers[i]) into
            // UpdateController() in the delegate, they'll take the values they have at the
            // very end of the loop (so by the time the delegate gets called, i = vertices.Length and stuff)
            Controller freezeController = newControllers[i];
            int freezeIndex = i;
            newDelegates[i] = delegate (TransformNotifier.Transform t)
            {
                UpdateController(t, freezeIndex, freezeController);
            };
            newControllers[i].notifier.NewValue += newDelegates[i];
        }

        // yeet any surplus controllers (rip)
        for (int i = newControllers.Length; i < controllers.Length; i++)
        {
            controllers[i].notifier.NewValue -= delegatesToDelete[i];
            Destroy(controllers[i]);
        }

        controllers = newControllers;
        delegatesToDelete = newDelegates;
    }

    public void UpdateController(TransformNotifier.Transform t, int ogIndex, Controller ogController)
    {
        Vector3[] v = meshFilter.mesh.vertices;
        v[ogIndex] = ogController.transform.localPosition;

        if (symmetry)
        {
            // update all the controllers in the same row
            for (int i = 0; i < controllers.Length; i++)
            {
                // don't update this controller again
                if (ReferenceEquals(controllers[i], ogController))
                {
                    continue;
                }
                // check if same row
                if (i / resolution.value == ogIndex / resolution.value)
                {
                    // FIXME: gotta somehow multiply by some component of ogController.transform??
                    controllers[i].MoveSilentlyBy(t.vector);
                }
                v[i] = controllers[i].transform.localPosition;
            }
        }

        meshFilter.mesh.SetVertices(v);
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
                controllers[i].Hide();
            }
            visible = false;
        }
    }

    //updates the normals hackily
    void Update() {
        if(controllers != null) {
            for(int i = 0; i < controllers.Length; i++) {
                controllers[i].transform.forward = meshFilter.mesh.normals[i];
            }
        }
    }
}
