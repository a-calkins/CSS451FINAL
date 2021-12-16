using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string normalObstacleTag;
    public bool negative;
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
        int rot = (int) Mathf.Abs(transform.localEulerAngles.y);
        transform.localPosition = new Vector3(
            transform.localPosition.x + ((rot % 180 == 0) ? 2 : 0) * (negative ? -1 : 1),
            2,
            transform.localPosition.z + ((rot % 90 == 0 && rot % 180 != 0) ? 2 : 0) * (negative ? -1 : 1)
        );
        transform.localRotation *= Quaternion.Euler(0, 90, 0);
    }
}
