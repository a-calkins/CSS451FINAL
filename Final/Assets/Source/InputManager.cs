using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public SceneNode[] players;

    public enum Key
    {
        Up,
        Down,
        Left,
        Right
    };

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (SceneNode player in players)
        {
            if (Input.GetKeyDown(player.GetControl(Key.Up)))
            {
                player.Move(0, 1);
            }
            if (Input.GetKeyDown(player.GetControl(Key.Down)))
            {
                player.Move(0, -1);
            }
            if (Input.GetKeyDown(player.GetControl(Key.Left)))
            {
                player.Move(-1, 0);
            }
            if (Input.GetKeyDown(player.GetControl(Key.Right)))
            {
                player.Move(1, 0);
            }
        }
    }
}
