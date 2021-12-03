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
        dx = mouseX - Input.mousePosition.x;
        dy = mouseY - Input.mousePosition.y;
        mouseX = Input.mousePosition.x;
        mouseY = Input.mousePosition.y;
        
        // something that's been bugging me in the example solution is how the axis movement goes
        // totally out of whack if the camera is rotated
        // this is an attempt to do things differently? and ideally it'd be cool/correct to
        // incorporate both dx and dy but i can't quite figure it out atm (for example an axis
        // that's diagonal onscreen should move equal amounts if the mouse goes either sideways or downwards)
        float screenRight = Vector3.Project(Camera.main.transform.right, selectedAxis.transform.up).magnitude;
        float screenUp = Vector3.Project(Camera.main.transform.up, selectedAxis.transform.up).magnitude;

        // so for now we just take the one component (out of dx and dy) that contributes 'most' to the
        // axis's movement, i.e. the one that overlaps more with the axis's up direction
        // and also figure out whether to negate it or not based on where the camera is in relation to the axis
        float directionScale = (screenRight > screenUp) switch
        {
            true => dx * screenRight * -Mathf.Sign(Vector3.Dot(Camera.main.transform.right, selectedAxis.transform.up)),
            false => dy * screenUp * -Mathf.Sign(Vector3.Dot(Camera.main.transform.up, selectedAxis.transform.up))
        };

        selected.transform.localPosition += directionScale * tracking * selectedAxis.transform.up;

        /*
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
        */
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
