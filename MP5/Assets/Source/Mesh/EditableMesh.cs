using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(TexturePlacement))]
public class EditableMesh : MonoBehaviour
{
    // turns off half the faces to make the vertices easier to see
    public bool debugVertices;
    private Quaternion currentRot = Quaternion.identity;
    private MeshFilter meshFilter;
    private SliderWithEcho.Values resolution;
    private SliderWithEcho.Values size;
    new public string name;
    public bool symmetry { get; private set; }

    //array to hold the controllers (spheres)
    private Controller[] controllers = new Controller[0];
    public bool visible;

    private Matrix3x3 uvTransform = Matrix3x3.identity;
    private Vector2[] originalUVs;

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

    private void SetMesh(Vector3[] vertices, int[] triangles, Vector3[] normals, Vector2[] uv)
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
        originalUVs = uv;
        UpdateUVs();
        UpdateControllers(vertices, normals);
    }

    public void UpdateTransform(Matrix3x3 transform)
    {
        uvTransform = transform;
        UpdateUVs();
    }

    public void UpdateUVs()
    {
        Vector2[] uv = originalUVs;
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = uvTransform * uv[i];
        }
        meshFilter.mesh.SetUVs(0, uv);
    }

    public void SetMesh(MeshTypes.Mesh mesh)
    {
        symmetry = mesh.symmetry;
        SetMesh(mesh.vertices, mesh.triangles, mesh.normals, mesh.uv);
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
            newDelegates[i] = delegatesToDelete[i];
            newControllers[i] = controllers[i]
                .MoveSilentlyTo(vertices[i])
                .SetRotation(normals[i])
                .HideAxes()
                .ShowIf(visible);
        }

        // fill in any controllers we still need to create
        for (int i = controllers.Length; i < newControllers.Length; i++) {
            newControllers[i] = Instantiate(Resources.Load("Prefabs/Controller") as GameObject, transform)
                .GetComponent<Controller>()
                .MoveSilentlyTo(vertices[i])
                .SetRotation(normals[i])
                .HideAxes()
                .ShowIf(visible);

            // we have to 'freeze' these values within this iteration of the loop because
            // if we just do UpdateController(i, newControllers[i]) in the delegate, they'll
            // take the values they have at the very end of the loop (so by the time the
            // delegate gets called, we'll have i = vertices.Length and controllers[i]
            // will be out of bounds)
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
            Destroy(controllers[i].gameObject);
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
            UpdateNeighborsAboveBelow(ogIndex);
            int rowStart = resolution.value * (ogIndex / resolution.value);
            // update all the controllers in the same row
            for (int i = rowStart; i < rowStart + resolution.value; i++)
            {
                // don't update this controller again
                if (i == ogIndex)
                {
                    continue;
                }
                controllers[i].MoveSilentlyBy(
                    // align the right axis with the normal
                    Quaternion.FromToRotation(-Vector3.forward, Vector3.Cross(Vector3.up, meshFilter.mesh.normals[i]))
                    * t.vector
                );
                // TODO: if we want generalized x/y symmetry this AboveBelow function has to be
                // replaced with something smarter
                UpdateNeighborsAboveBelow(i);
                UpdateControllerNormal(i, true);
                v[i] = controllers[i].transform.localPosition;
            }
        } else
        {
            UpdateNeighbors(ogIndex);
        }

        UpdateControllerNormal(ogIndex, true);

        meshFilter.mesh.SetVertices(v);

        // update all normals (setting them individually wasn't working!!)
        Vector3[] n = new Vector3[controllers.Length];
        for (int i = 0; i < n.Length; i++)
        {
            n[i] = controllers[i].Normal;
        }
        meshFilter.mesh.SetNormals(n);
    }

    // update the normals of all of this controller's neighbors
    private void UpdateNeighbors(int index)
    {
        int north = index + resolution.value,
            east = index + 1,
            south = index - resolution.value,
            west = index - 1;

        int northeast = north + 1,
            southeast = south + 1,
            southwest = south - 1,
            northwest = north - 1;

        // these conditions are used to determine whether or not there's actually
        // a usable vertex at whatever compass direction
        bool northCond = index / resolution.value < resolution.value,
            eastCond = index % resolution.value < resolution.value - 1,
            southCond = index / resolution.value > 0,
            westCond = index % resolution.value > 0;

        // first pass: update the controllers' normal cylinders only,
        // without touching the actual normals of the mesh
        // (this way the controllers can all update themselves based on the
        // original state of the mesh without messing it and themselves up)
        UpdateControllerNormal(north, northCond);
        UpdateControllerNormal(northeast, northCond && eastCond);
        UpdateControllerNormal(east, eastCond);
        UpdateControllerNormal(southeast, southCond && eastCond);
        UpdateControllerNormal(south, southCond);
        UpdateControllerNormal(southwest, southCond && westCond);
        UpdateControllerNormal(west, westCond);
        UpdateControllerNormal(northwest, northCond && westCond);

        // second pass to be done AFTER this method is called for all controllers:
        // now that they all have their new normals set, apply those changes to the mesh
    }

    // hacky symmetry thing, probably don't have time to make this
    // more general (eg different axes) so it'll do lol -- just ignores the
    // westward and eastward neighbors of every controller in the cylinder
    // bc there's no need to update them, only the ones directly north and south
    private void UpdateNeighborsAboveBelow(int index)
    {
        int north = index + resolution.value,
            south = index - resolution.value;

        bool northCond = index / resolution.value < resolution.value,
            southCond = index / resolution.value > 0;

        // see comments in UpdateNeighbors()
        UpdateControllerNormal(north, northCond);
        UpdateControllerNormal(south, southCond);
    }

    // updates the normal of this individual controller,
    // but only if condition is true (used by the caller to determine
    // whether or not a controller/vertex actually exists at the index)
    private void UpdateControllerNormal(int index, bool condition)
    {
        if (!condition)
        {
            return;
        }
        int north = index + resolution.value,
            east = index + 1,
            south = index - resolution.value,
            west = index - 1;

        int northeast = north + 1,
            southeast = south + 1,
            southwest = south - 1,
            northwest = north - 1;

        bool northCond = index / resolution.value < resolution.value,
            eastCond = index % resolution.value < resolution.value - 1,
            southCond = index / resolution.value > 0,
            westCond = index % resolution.value > 0;

        // kelvin's code was a bit more efficient in that it didn't do all 6 calculations
        // for every single vertex (it joined neighbors up instead) but this works hah
        controllers[index].SetNormal(
            -(
                GetFaceNormal(west, westCond, index, true, northwest, northCond && westCond) +
                GetFaceNormal(northwest, westCond, index, true, north, northCond) +
                GetFaceNormal(index, true, east, eastCond, north, northCond) +
                /* this is the northwesternmost face and it doesn't touch the center vertex */
                // GetFaceNormal(north, northCond, east, eastCond, northeast, northCond && eastCond) +
                /* this is the southeasternmost face and it doesn't touch the center vertex */
                // GetFaceNormal(southwest, southCond && westCond, south, southCond, west, westCond) +
                GetFaceNormal(west, westCond, south, southCond, index, true) +
                GetFaceNormal(south, southCond, southeast, southCond && eastCond, index, true) +
                GetFaceNormal(index, true, southeast, southCond && eastCond, east, eastCond)
            ).normalized
        );
    }

    // gets the normal of the face defined by these three vertices
    // and condA condB condC are used to determine whether their corresponding
    // vertex actually exists -- if it doesn't, then it makes do with whatever
    // vertices actually do exist
    private Vector3 GetFaceNormal(int a, bool condA, int b, bool condB, int c, bool condC)
    {
        // this code is hideous
        Vector3 backup = Vector3.zero;
        if (condA)
        {
            backup = controllers[a].transform.localPosition;
        }
        else if (condB)
        {
            backup = controllers[b].transform.localPosition;
        }
        else if (condC)
        {
            backup = controllers[c].transform.localPosition;
        }

        Vector3 posA = condA ? controllers[a].transform.localPosition : backup;
        Vector3 posB = condB ? controllers[b].transform.localPosition : backup;
        Vector3 posC = condC ? controllers[c].transform.localPosition : backup;

        return Vector3.Cross(posB - posA, posC - posA).normalized;
    }

    //make the controllers visible and interactable
    public void ShowControllers() {
        if(controllers != null && !visible) {
            foreach (Controller c in controllers) {
                c.Show();
            }
            visible = true;
        }
    }

    //hide the controllers and make them uninteractable
    public void HideControllers() {
        if(controllers != null && visible) {
            foreach (Controller c in controllers) {
                c.Hide();
            }
            visible = false;
        }
    }
}
