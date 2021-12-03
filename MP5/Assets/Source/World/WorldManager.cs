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
    private GameObject selected, selectedAxis;
    RaycastHit hitInfo = new RaycastHit();
    Ray ray;
    Color original;
    bool visible;
    float mouseX = 0, mouseY = 0, dy, dx, tracking = 0.05f;

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
            SelectObject();
            if(selected != null && selectedAxis == null) {
                mouseX = Input.mousePosition.x;
                mouseY = Input.mousePosition.y;
                SelectAxis();
            }
        }
        if(Input.GetMouseButton(0) && visible)
        {
            if(selectedAxis == null) {
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
                if(hitInfo.collider.tag == "Controller")
                {
                    if(selected == null) {
                        selected = hitInfo.collider.gameObject;
                        selected.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        for(int i = 0; i < 3; i++) {
                            selected.transform.GetChild(i).GetComponent<Renderer>().enabled = true;
                        }
                    }
                    else if(selected != null && hitInfo.collider.gameObject == selected) {
                        Deselect();
                    }
                    else {
                        SelectNew();
                    } 
                }  
            }
        }
    }
    void SelectNew() {
        selected.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        for(int i = 0; i < 3; i++) {
            selected.transform.GetChild(i).GetComponent<Renderer>().enabled = false;
        }
        selected = hitInfo.collider.gameObject;
        selected.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
        for(int i = 0; i < 3; i++) {
            selected.transform.GetChild(i).GetComponent<Renderer>().enabled = true;
        }
    }
    //selects an axis on the controller after it is made visible
    void SelectAxis() {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitInfo))
            {
                if(hitInfo.collider.tag == "x" || hitInfo.collider.tag == "y" || hitInfo.collider.tag == "z")
                {
                    selectedAxis = hitInfo.collider.gameObject;
                    original = selectedAxis.GetComponent<Renderer>().material.color;
                    selectedAxis.GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.5f));
                }
                
            }
        }
    }
    //drags the object
    void DragObject() {
        Vector3 delta = new Vector3(0,0,0);
        if(selectedAxis.tag == "x") {
            dx = mouseX - Input.mousePosition.x;
            mouseX = Input.mousePosition.x;
            delta = -dx * tracking * transform.right;
        }
        else if(selectedAxis.tag == "y") {
            dy = mouseY - Input.mousePosition.y;
            mouseY = Input.mousePosition.y;
            delta = -dy * tracking * transform.up;
        }
        else if(selectedAxis.tag == "z"){
            //use dy since there is no z for the mouse
            dy = mouseY - Input.mousePosition.y;
            mouseY = Input.mousePosition.y;
            delta = -dy * tracking * transform.forward;
        }
        selected.transform.localPosition += delta;
    }
    //deselects the currently selected object
    void Deselect()
    {
        selected.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        for(int i = 0; i < 3; i++) {
            selected.transform.GetChild(i).GetComponent<Renderer>().enabled = false;
        }
        selected = null;
    }
    void DeselectAxis() {
        selectedAxis.GetComponent<Renderer>().material.SetColor("_Color", original);
        selectedAxis = null;
    }
}
