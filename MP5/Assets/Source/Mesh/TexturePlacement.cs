using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TexturePlacement : MonoBehaviour
{
    public float debugNormals = 0f;  // scale from 0 to 1 = opacity of normal-debugging layer
    public Vector2 offset = Vector2.zero;
    public Vector2 scale = Vector2.one;
    public Quaternion rotation = Quaternion.identity;

    private new Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
        renderer.material.SetFloat("MyTexOffset_X", offset.x);
        renderer.material.SetFloat("MyTexOffset_Y", offset.y);

        renderer.material.SetFloat("MyTexScale_X", scale.x);
        renderer.material.SetFloat("MyTexScale_Y", scale.y);
        

        renderer.material.SetFloat("debugNormals", debugNormals);
    }
}