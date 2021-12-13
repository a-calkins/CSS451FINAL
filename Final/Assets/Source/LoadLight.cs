using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LoadLight : MonoBehaviour {
    public SceneNode LightPosition;
    private new Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        renderer.material.SetVector("LightPosition", new Vector3(LightPosition.absolutePosition.x, 3, LightPosition.absolutePosition.y));
    }
}
