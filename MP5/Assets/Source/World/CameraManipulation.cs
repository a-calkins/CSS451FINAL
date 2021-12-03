using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManipulation : MonoBehaviour
{
    float delta = 0;
    float scrollSpeed = 10f;
    const float tiltAngle = 0.1f;
    const float trackingDistance = 0.05f;
    float mouseClickX = 0, mouseClickY = 0;
    public Transform LookAtPosition;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //look at the look at position
        transform.up = Vector3.up;
        transform.forward = (LookAtPosition.transform.localPosition - transform.localPosition).normalized;

        //gets the mouse click position
        if (Input.GetKey(KeyCode.LeftAlt) &&
            (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {

            mouseClickX = Input.mousePosition.x;
            mouseClickY = Input.mousePosition.y;
        }
        else if (Input.GetKey(KeyCode.LeftAlt) &&
            (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {

            float moveAroundX = mouseClickX - Input.mousePosition.x;
            float moveAroundY = mouseClickY - Input.mousePosition.y;

            mouseClickX = Input.mousePosition.x;
            mouseClickY = Input.mousePosition.y;

            //handle rotation
            if (Input.GetMouseButton(0))
            {
                RotateVertical(-moveAroundX * tiltAngle);
                RotateHorizontal(moveAroundY * tiltAngle);
            }
            //handle tracking
            else if(Input.GetMouseButton(1)) {
                Vector3 delta = moveAroundX * trackingDistance * transform.right + moveAroundY * trackingDistance * transform.up;
                LookAtPosition.localPosition += delta;
                transform.localPosition += delta;  
            }
        }

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            //zoom with mouse scrollwheel
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                delta -= (Input.GetAxis("Mouse ScrollWheel") * scrollSpeed);
                delta = Mathf.Clamp(delta, -1f, 1f);
                ProcessZoom(delta);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                delta -= (Input.GetAxis("Mouse ScrollWheel") * scrollSpeed);
                delta = Mathf.Clamp(delta, -1f, 1f);
                ProcessZoom(delta);
            }
        }
    }
    //rotates about y-axis
    void RotateVertical(float angle)
    {
        Quaternion rotUp = Quaternion.AngleAxis(angle, transform.up);
        ProcessRotation(rotUp);
    }
    //rotates about x-axis
    void RotateHorizontal(float angle)
    {
        Quaternion rotSide = Quaternion.AngleAxis(angle, transform.right);
        ProcessRotation(rotSide);
    }

    void ProcessRotation(Quaternion rot)
    {
        //rotation matrix
        Matrix4x4 rotation = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
        //inverse matrix of look at position, modifies transform value only
        Matrix4x4 invLookAt = Matrix4x4.TRS(-LookAtPosition.localPosition, Quaternion.identity, Vector3.one);
        //combine the two and multiply by the inverse's inverse
        Matrix4x4 m = (invLookAt.inverse * rotation) * invLookAt;
        //transform the camera's position based on the defined matrix above
        Vector3 newPos = m.MultiplyPoint(transform.localPosition);

        //limit the amount the camera can be tilted up or down, check if almost perpendicular to the y-axis/up direction
        if (Mathf.Abs(Vector3.Dot(newPos.normalized, Vector3.up)) < 0.985f)
        {
            //update camera pos
            transform.localPosition = newPos;
            //look at direction
            Vector3 v = (LookAtPosition.localPosition - transform.localPosition).normalized;
            //direction perpendicular to v and the up vector
            Vector3 w = Vector3.Cross(v, transform.up).normalized;
            //direction perpendicular to v and w
            Vector3 u = Vector3.Cross(w, v).normalized;
            //w will be set to the camera's transform.right, and u will be set to the camera's transform.up
            transform.up = u;
            transform.right = w;
            transform.forward = v;
            //set forward last for the rotation to work correctly.
        }
    }
    //zooms in and out based on scroll wheel delta
    void ProcessZoom(float delta)
    {
        Vector3 v = LookAtPosition.localPosition - transform.localPosition;
        float dist = v.magnitude;
        dist += delta;
        dist = Mathf.Clamp(dist, 5, 25);
        transform.localPosition = LookAtPosition.localPosition - dist * v.normalized;
    }
}