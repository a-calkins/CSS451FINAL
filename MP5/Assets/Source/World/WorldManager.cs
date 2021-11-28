using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public EditableMesh mesh;

    // ugly :X these Notifiers are just weird wrappers for delegates
    public StringNotifier dropdown;
    public TransformNotifier textureTransform;
    // thinking of just having one slider for all meshes' resolutions
    // (we could still keep track of different meshes' resolutions separately,
    // just without showing two sliders at all times?)
    public FloatNotifier resolution;
    // we can control the plane's size too, why not
    public FloatNotifier length;

    // Start is called before the first frame update
    void Start()
    {
        dropdown.Notifier += delegate (string name)
        {
            MakeNewMesh(name, resolution.current, length.current);
        };
        resolution.Notifier += delegate (float value)
        {
            MakeNewMesh(mesh.name, value, length.current);
        };
        length.Notifier += delegate (float value)
        {
            MakeNewMesh(mesh.name, resolution.current, value);
        };
        MakeNewMesh("mesh", resolution.current, length.current);
    }

    private void MakeNewMesh(string name, float resolution, float length)
    {
        var generated = StaticMeshGenerator.Generate(
            name.ToLowerInvariant(),
            (int)resolution,
            (int)length
        );
        mesh.UpdateMesh(generated.vertices, generated.triangles, generated.normals);
        mesh.name = name;
    }
}
