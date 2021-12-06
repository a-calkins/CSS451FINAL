using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string normalObstacleTag;
    private bool interacted = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        if (interacted)
        {
            return;
        }

        interacted = true;
        gameObject.tag = normalObstacleTag;

        // todo: behave differently per prefab
        transform.localRotation = Quaternion.Euler(0, 90, -90);
        transform.localPosition = new Vector3(transform.localPosition.x + 2, 2, transform.localPosition.z);
    }
}
