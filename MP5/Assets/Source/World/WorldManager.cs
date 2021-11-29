using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public EditableMesh obj;

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
            SetSliders(name);
            SetMesh(name, resolution.current, size.current);
        };
        resolution.NewValue += delegate (float value)
        {
            SetMesh(obj.name, value, size.current);
            obj.Resolution((int)value);
        };
        size.NewValue += delegate (float value)
        {
            SetMesh(obj.name, resolution.current, value);
            obj.Size((int)value);
        };

        // initialize 
        SetSliders("mesh");
        SetMesh("mesh", resolution.current, size.current);
    }

    private void SetMesh(string name, float resolution, float length)
    {
        name = name.ToLowerInvariant();  // make the name case-insensitive
        obj.SetMesh(
            MeshTypes.Generate(
                name,
                (int)resolution,
                (int)length
            )
        );
        obj.name = name;
    }

    private void SetSliders(string name)
    {
        name = name.ToLowerInvariant();  // make the name case-insensitive
        var resolution = MeshTypes.GetResolutionValues(name);
        var size = MeshTypes.GetSizeValues(name);

        obj.ChangeSliders(resolution, size);
        resolutionSlider.ChangeSilently(resolution);
        sizeSlider.ChangeSilently(size);
    }
}
