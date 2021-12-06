using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SceneNode))]
public class Root : MonoBehaviour
{
    private SceneNode root;
    // Start is called before the first frame update
    void Start()
    {
        root = GetComponent<SceneNode>(); ;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos, dir;
        Matrix4x4 i = Matrix4x4.identity;
        root.CompositeTransform(ref i, out pos, out dir);
    }
}
