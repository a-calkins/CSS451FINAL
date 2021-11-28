using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StringNotifier))]
[RequireComponent(typeof(Dropdown))]
public class MeshSelector : MonoBehaviour
{
    private StringNotifier notifier;
    private Dropdown dropdown;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<Dropdown>();
        notifier = GetComponent<StringNotifier>();

        dropdown.onValueChanged.AddListener(delegate
        {
            notifier.UpdateValue(dropdown.options[dropdown.value].text);
        });
    }
}
