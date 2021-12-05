using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

public class WorldManager : MonoBehaviour
{
    public EditableMesh obj;

    // ugly :X these Notifiers are just weird wrappers for delegates
    public StringNotifier dropdown;
    public TransformNotifier textureTransform;
    public FloatNotifier resolution;
    private SliderWithEcho resolutionSlider;
    public FloatNotifier size;
    private SliderWithEcho sizeSlider;

    //vertex selection and manipulation stuff
    private Controller selected;
    private Renderer selectedAxis;
    RaycastHit hitInfo = new RaycastHit();
    Ray ray;
    Color original;
    bool visible;
    float mouseX = 0, mouseY = 0, dy, dx, tracking = 0.01f;

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
        // this feels hacky, it's only here to stop newly instantiated
        // controllers from disappearing (since they inherit obj.visible)
        // should remove this if we can do it 100% within obj instead
        obj.visible = visible;
        
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            visible = true;
            obj.ShowControllers();
        }
        if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            visible = false;
            obj.HideControllers();
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            return;
        }
        if(Input.GetMouseButtonDown(0) && visible) {
            SelectObject();
            if(selected != null && selectedAxis == null)
            {
                mouseX = Input.mousePosition.x;
                mouseY = Input.mousePosition.y;
                SelectAxis();
            }
        }
        if(Input.GetMouseButton(0) && visible)
        {
            if(selectedAxis == null)
            {
                mouseX = Input.mousePosition.x;
                mouseY = Input.mousePosition.y;
                SelectAxis();
            }
            if(selectedAxis != null && selected != null)
                DragObject();      
        }
        if(Input.GetMouseButtonUp(0) && selectedAxis != null) {
            DeselectAxis();
        }
        if(selected != null && !visible) {
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
                if(hitInfo.collider.CompareTag("Controller"))
                {
                    if(selected == null) {
                        selected = hitInfo.collider.gameObject.GetComponent<Controller>();
                        selected.Select();
                    }
                    else if(selected != null && hitInfo.collider.gameObject == selected) {
                        selected.Deselect();
                    }
                    else {
                        SelectNew();
                    } 
                }  
            }
        }
    }

    void SelectNew() {
        selected.Deselect();
        selected = hitInfo.collider.gameObject.GetComponent<Controller>();
        selected.Select();
    }

    //selects an axis on the controller after it is made visible
    void SelectAxis() {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitInfo))
            {
                if(hitInfo.collider.CompareTag("x") || hitInfo.collider.CompareTag("y") || hitInfo.collider.CompareTag("z"))
                {
                    selectedAxis = hitInfo.collider.gameObject.GetComponent<Renderer>();
                    original = selectedAxis.material.color;
                    selectedAxis.material.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.5f));
                }
                
            }
        }
    }

    //drags the object
    void DragObject() {
        dx = mouseX - Input.mousePosition.x;
        dy = mouseY - Input.mousePosition.y;
        mouseX = Input.mousePosition.x;
        mouseY = Input.mousePosition.y;

        // something that's been bugging me in the example solution is how the axis movement goes
        // totally out of whack if the camera is rotated
        // this is an attempt to do things differently? trying to take screen space into account
        // so that e.g. if an axis is pointing left onscreen, moving the mouse left always moves the
        // selected controller along that axis
        // or if an axis is diagonal onscreen, moving the mouse either up or sideways moves the controller
        // by an equal amount

        // get the scalar projection between the camera's (screen's) axes and the object's current axis
        float screenRight = -Vector3.Dot(Camera.main.transform.right, selectedAxis.transform.up);
        float screenUp = -Vector3.Dot(Camera.main.transform.up, selectedAxis.transform.up);

        // add together the individual components of the mouse movement,
        // weighted by how much they 'contribute' to movement along the axis
        // (i.e. weighted by how much they overlap with the axis's up direction)
        float directionScale = dx * screenRight + dy * screenUp;
        
        selected.MoveBy(directionScale * tracking * selectedAxis.transform.up);
    }

    //deselects the currently selected object
    void Deselect()
    {
        selected.Deselect();
        selected = null;
    }

    void DeselectAxis() {
        selectedAxis.material.SetColor("_Color", original);
        selectedAxis = null;
    }
}
