using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    //vertex selection and manipulation stuff
    private GameObject selected;
    RaycastHit hitInfo = new RaycastHit();
    Ray ray;
    bool visible;
    float mouseX = 0, mouseY = 0, tracking = 0.05f;

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

    void Update() {
        if(Input.GetKeyDown(KeyCode.LeftControl)) {
            obj.ShowControllers();
            visible = true;
        }
        if(Input.GetKeyUp(KeyCode.LeftControl)) {
            obj.HideControllers();
            visible = false;
        }
        if(Input.GetMouseButtonDown(0) && visible) {
            if(selected == null) {
                SelectObject();
            }
            mouseX = Input.mousePosition.x;
            mouseY = Input.mousePosition.y;
        }
        if(Input.GetMouseButton(0) && visible)
        {
            if(selected == null)
                SelectObject();
            else
                DragObject();      
        }
        if((Input.GetMouseButtonUp(0) && selected != null) || !visible) {
            Deselect();
        } 
    }

    //selected a valid object
    void SelectObject()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitInfo))
            {
                if(hitInfo.collider.tag == "Controller")
                {
                    selected = hitInfo.collider.gameObject;
                }
                
            }
        }
    }
    //drags the object
    void DragObject() {
        float dx = mouseX - Input.mousePosition.x;
        float dy = mouseY - Input.mousePosition.y;
        mouseX = Input.mousePosition.x;
        mouseY = Input.mousePosition.y;

        Vector3 delta = -dx * tracking * transform.right + -dy * tracking * transform.up;
        selected.transform.localPosition += delta;
    }
    //deselects the currently selected object
    void Deselect()
    {
        selected = null;
    }
}
