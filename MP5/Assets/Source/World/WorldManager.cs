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
    private SliderWithEcho resolutionSlider;
    // we can control the plane's size too, why not
    public FloatNotifier size;
    private SliderWithEcho sizeSlider;

    void Awake()
    {
        resolutionSlider = resolution.GetComponent<SliderWithEcho>();
        sizeSlider = size.GetComponent<SliderWithEcho>();
    }

    void Start()
    {
        dropdown.NewValue += delegate (string name)
        {
            ChangeSliders(name);
            ChangeMesh(name, resolution.current, size.current);
        };
        resolution.NewValue += delegate (float value)
        {
            ChangeMesh(mesh.name, value, size.current);
            mesh.Resolution((int)value);
        };
        size.NewValue += delegate (float value)
        {
            ChangeMesh(mesh.name, resolution.current, value);
            mesh.Size((int)value);
        };
        ChangeMesh("mesh", resolution.current, size.current);
    }

    private void ChangeMesh(string name, float resolution, float length)
    {
        name = name.ToLowerInvariant();  // make the name case-insensitive
        mesh.ChangeMesh(
            MeshTypes.Generate(
                name,
                (int)resolution,
                (int)length
            )
        );
        mesh.name = name;
    }

    private void ChangeSliders(string name)
    {
        name = name.ToLowerInvariant();  // make the name case-insensitive
        var resolution = MeshTypes.GetResolutionValues(name);
        var size = MeshTypes.GetSizeValues(name);

        mesh.ChangeSliders(resolution, size);
        resolutionSlider.ChangeSilently(resolution);
        sizeSlider.ChangeSilently(size);
    }
}
