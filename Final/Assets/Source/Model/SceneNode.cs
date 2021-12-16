using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// TODO: neeeeeeed to factor out player-related stuff into its own class
[ExecuteInEditMode]
public class SceneNode : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public KeyCode[] upDownLeftRight = new KeyCode[4]
    {
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow
    };
    public bool rotateInPlace;
    public bool isFirstPerson;
    public bool visible;
    public bool isKey;
    public string collisionTag;
    public string interactableTag;
    public int moveBy = 2;

    public new Camera camera;
    public Vector3 cameraOrigin = Vector3.zero;
    public Vector3 cameraAxis = Vector3.right;
    public float cameraAngle = default;

    protected Matrix4x4 mCombinedParentXform;
    private Direction direction = Direction.Up;
    private Direction referenceDirection = Direction.Up;  // parent's direction
    
    public Transform AxisFrame;
    public Vector3 NodeOrigin = Vector3.zero;
    public List<NodePrimitive> PrimitiveList;
    private List<SceneNode> children = new List<SceneNode>();

    private bool hasParent = false;
    private bool parentMoved = true;  // this lets child scenenodes not move more than one step until their parent moves again
    private bool movedSinceParent = false;

    private float currentRotation;

    public Vector2 absolutePosition { get; private set; } = Vector2.zero;

    const float kAxisFrameSize = 5f;

    private int rotInterpStepsLeft = 0;
    private int rotInterpStart = 0;
    private int rotInterpTarget = 0;

    private int transInterpStepsLeft = 0;
    private int transInterpStart = 0;
    private int transInterpTarget = 0;

    static int INTERPOLATION_STEPS = 25;

    void Awake()
    {
        UnSelect();
        Debug.Assert(AxisFrame != null);
        InitializeSceneNode();
        foreach (Transform child in transform)
        {
            SceneNode childNode = child.GetComponent<SceneNode>();
            if (childNode != null)
            {
                children.Add(childNode);
            }
        }
        if (transform.parent != null) // if this isn't the root
        {
            foreach (SceneNode childNode in children)
            {
                childNode.SetHasParent();
            }
        }
    }

    // Use this for initialization
    protected void Start()
    {
        // Debug.Log("PrimitiveList:" + PrimitiveList.Count);
    }

    private void SetHasParent()
    {
        hasParent = true;
    }

    private void SetParentMoved()
    {
        parentMoved = true;
        movedSinceParent = false;
        foreach (SceneNode child in children)
        {
            child.SetParentMoved();
        }
    }

    public KeyCode GetControl(InputManager.Key c)
    {
        return upDownLeftRight[(int)c];
    }

    static (int, int) DirectionToTuple(Direction d)
    {
        return d switch
        {
            Direction.Up => (0, 1),
            Direction.Left => (-1, 0),
            Direction.Down => (0, -1),
            Direction.Right => (1, 0),
            _ => (0, 0)
        };
    }

    static Direction TupleToDirection((int, int) t)
    {
        return t switch
        {
            (0, 1) => Direction.Up,
            (-1, 0) => Direction.Left,
            (0, -1) => Direction.Down,
            (1, 0) => Direction.Right,
            _ => throw new System.Exception("bad direction " + t)
        };
    }

    static Direction RotateBy(Direction a, Direction b)
    {
        return b switch
        {
            Direction.Up => a,
            Direction.Left => (Direction)(((int)a + 3) % 4),
            Direction.Down => (Direction)(((int)a + 2) % 4),
            Direction.Right => (Direction)(((int)a + 1) % 4),
            _ => a
        };
    }

    public void Move(int x, int y)
    {
        if (hasParent && !parentMoved)
        {
            return;
        }

        Direction rawDirection = TupleToDirection((x, y));

        // this is a hack to target the blue thing because it has a different control style from the red one
        if (isFirstPerson)
        {
            direction = RotateBy(direction, rawDirection);
        }
        else
        {
            direction = RotateBy(rawDirection, referenceDirection);
            //direction = rawDirection;
        }

        (int newx, int newy) = DirectionToTuple(direction);
        // camera == null is a hack to target the blue maze thingy!!! remove later
        // (it allows the blue one to move while turning but forces the red one to turn without moving)
        if ((!isFirstPerson || rawDirection == Direction.Up) && !ObstacleAt(newx, newy))
        {
            NodeOrigin = new Vector3(
                NodeOrigin.x + newx * moveBy,
                NodeOrigin.y,
                NodeOrigin.z + newy * moveBy
            );
            parentMoved = false;
            foreach (SceneNode child in children)
            {
                child.SetParentMoved();
            }
            movedSinceParent = true;
        }

    }

    private bool ObstacleAt(int x, int y)
    {

        float absoluteX = absolutePosition.x;
        float absoluteY = absolutePosition.y;
        float newX = absoluteX + x * moveBy;
        float newY = absoluteY + y * moveBy;

        foreach (SceneNode child in children)
        {
            if (child.ObstacleAt(x, y))
            {
                // lol linear search
                foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag(collisionTag))
                {
                    if (
                        (Mathf.Abs(obstacle.transform.position.x - newX) < moveBy * .9) &&
                        (Mathf.Abs(obstacle.transform.position.z - newY) < moveBy * .9)
                    )
                    {
                        foreach (NodePrimitive p in PrimitiveList)
                        {
                            p.FlashBlack();
                        }
                    }
                }
                return true;
            }
        }

        // lol linear search
        foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag(collisionTag))
        {
            if (
                (Mathf.Abs(obstacle.transform.position.x - newX) < moveBy * .9) &&
                (Mathf.Abs(obstacle.transform.position.z - newY) < moveBy * .9)
            )
            {
                foreach (NodePrimitive p in PrimitiveList)
                {
                    p.FlashBlack();
                }
                return true;
            }
        }

        foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag(interactableTag))
        {
            if (
                (Mathf.Abs(obstacle.transform.position.x - newX) < moveBy * .9) &&
                (Mathf.Abs(obstacle.transform.position.z - newY) < moveBy * .9)
            )
            {
                obstacle.GetComponent<Interactable>().Interact();
                return false;
            }
        }

        if (!visible)
        {
            foreach(GameObject obstacle in GameObject.FindGameObjectsWithTag("Key"))
            {
                if (
                    (Mathf.Abs(obstacle.transform.position.x - newX) < moveBy * .9) &&
                    (Mathf.Abs(obstacle.transform.position.z - newY) < moveBy * .9)
                )
                {
                    visible = true;
                    obstacle.GetComponent<Renderer>().enabled = false;
                    return false;
                }
            }
        }

        if (isKey)
        foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag("Win"))
        {
            if (
                (Mathf.Abs(obstacle.transform.position.x - newX) < moveBy * .9) &&
                (Mathf.Abs(obstacle.transform.position.z - newY) < moveBy * .9)
            )
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #endif
                Application.Quit();
                }
        }

        return false;
    }

    public void SetToSelect() { AxisFrame.gameObject.SetActive(true); }
    public void UnSelect() { AxisFrame.gameObject.SetActive(false); }

    private void InitializeSceneNode()
    {
        mCombinedParentXform = Matrix4x4.identity;
    }
    static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion angles) {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = angles * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
     }

    // passing parentDirection is probably cheating but we can fix it after presentation
    public void CompositeTransform(ref Matrix4x4 parentXform, out Vector3 snOrigin, out Vector3 snUp, Direction parentDirection = Direction.Up)
    {
        if (parentDirection != referenceDirection)
        {
            referenceDirection = parentDirection;
            movedSinceParent = false;
        }

        Direction directionToUse = direction;
        if (!isFirstPerson && movedSinceParent)
        {
            directionToUse = RotateBy(direction, 4 - referenceDirection);
        }

        int angle = directionToUse switch
        {
            Direction.Up => 0,
            Direction.Right => 90,
            Direction.Down => 180,
            Direction.Left => -90,
            _ => 0
        };

        // all of this 180-wrangling stuff is to make it not turn 270 degrees the wrong way when it has to go
        // from e.g. -90 to +180 or something like that
        // code could be so much cleaner but not bothering :]
        if (angle != rotInterpTarget && !(angle == 180 && rotInterpTarget == -180 || angle == -180 && rotInterpTarget == 180))
        {
            rotInterpTarget = angle;
            rotInterpStart = (int)currentRotation;
            if (rotInterpStart == -90 && rotInterpTarget == 180)
            {
                rotInterpTarget = -180;
            }
            if (rotInterpStart == 90 && rotInterpTarget == -180)
            {
                rotInterpTarget = 180;
            }
            if (rotInterpStart == 180 && rotInterpTarget == -90)
            {
                rotInterpStart = -180;
            }
            if (rotInterpStart == -180 && rotInterpTarget == 90)
            {
                rotInterpStart = 180;
            }
            rotInterpStepsLeft = INTERPOLATION_STEPS;
        }

        float interpolatedAngle = (
            (rotInterpTarget - rotInterpStart) / (float)INTERPOLATION_STEPS
        ) * (INTERPOLATION_STEPS - rotInterpStepsLeft) + rotInterpStart;
        float angleToUse = rotInterpStepsLeft > 0 ? interpolatedAngle : angle;
        Quaternion angles = Quaternion.Euler(0, angleToUse, 0);

        Matrix4x4 orgT = Matrix4x4.Translate(NodeOrigin);
        Matrix4x4 trs = Matrix4x4.TRS(
            transform.localPosition,
            transform.localRotation,
            transform.localScale
        );

        if (rotateInPlace)
        {
            Matrix4x4 parentSansRotation = parentXform;
            Quaternion pq = Quaternion.identity;
            if (rotateInPlace)
            {
                // let's decompose the parent matrix into T, R, and S
                Vector3 pc0 = parentXform.GetColumn(0);
                Vector3 pc1 = parentXform.GetColumn(1);
                Vector3 pc2 = parentXform.GetColumn(2);
                Vector3 ps = new Vector3(pc0.magnitude, pc1.magnitude, pc2.magnitude);
                pq = Quaternion.LookRotation(pc2, pc1); // creates a rotation matrix with c2-Forward, c1-up
                Vector3 pp = parentXform.GetColumn(3);

                parentSansRotation = Matrix4x4.TRS(pp, Quaternion.identity, ps);
            }
            mCombinedParentXform = parentSansRotation * orgT * trs;
            mCombinedParentXform *= Matrix4x4.Rotate(pq);
        }
        else
        {
            mCombinedParentXform = parentXform * orgT * trs;
        }

        mCombinedParentXform *= Matrix4x4.Rotate(angles);
        
        // let's decompose the combined matrix into R, and S
        Vector3 c0 = mCombinedParentXform.GetColumn(0);
        Vector3 c1 = mCombinedParentXform.GetColumn(1);
        Vector3 c2 = mCombinedParentXform.GetColumn(2);
        Vector3 s = new Vector3(c0.magnitude, c1.magnitude, c2.magnitude);
        Quaternion q = Quaternion.LookRotation(c2, c1); // creates a rotation matrix with c2-Forward, c1-up

        snOrigin = mCombinedParentXform.GetColumn(3);
        snUp = c1;

        absolutePosition = new Vector2(snOrigin.x, snOrigin.z);

        if (camera != null && transform.parent != null)
        {
            Quaternion angleAxis = Quaternion.AngleAxis(cameraAngle, cameraAxis);
            camera.transform.localRotation = Quaternion.Euler(angleAxis.eulerAngles.x, angleToUse, angleAxis.eulerAngles.z);
            camera.transform.localPosition = RotatePointAroundPivot(cameraOrigin + snOrigin, snOrigin, angles);
        }

        currentRotation = angleToUse;

        if (rotInterpStepsLeft > 0)
        {
            rotInterpStepsLeft--;
        } else if (camera != null)
        {
            rotInterpStart = (int)camera.transform.localEulerAngles.y;
        }

        // propagate to all children
        foreach (SceneNode child in children)
        {
            child.CompositeTransform(ref mCombinedParentXform, out snOrigin, out snUp, direction);
        }

        // disenminate to primitives
        if (visible)
        foreach (NodePrimitive p in PrimitiveList)
        {
            p.SetGrayscale(!hasParent || hasParent && parentMoved);
            p.LoadShaderMatrix(ref mCombinedParentXform);
        }
    }
}