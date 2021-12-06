using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// TODO: neeeeeeed to factor out player-related stuff into its own class
[ExecuteInEditMode]
public class SceneNode : MonoBehaviour
{
    private enum Direction
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
    public string collisionTag;
    public string interactableTag;
    public int moveBy = 2;

    public new Camera camera;
    public Vector3 cameraOrigin = Vector3.zero;
    public Vector3 cameraAxis = Vector3.right;
    public float cameraAngle = default;

    protected Matrix4x4 mCombinedParentXform;
    private Direction direction = Direction.Up;
    
    public Transform AxisFrame;
    public Vector3 NodeOrigin = Vector3.zero;
    public List<NodePrimitive> PrimitiveList;
    private List<SceneNode> children = new List<SceneNode>();

    private bool hasParent = false;
    private bool parentMoved = true;  // this lets child scenenodes not move more than one step until their parent moves again

    private Vector2 absolutePosition = Vector2.zero;

    const float kAxisFrameSize = 5f;

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
	protected void Start () {
        // Debug.Log("PrimitiveList:" + PrimitiveList.Count);
	}

    private void SetHasParent()
    {
        hasParent = true;
    }

    private void SetParentMoved()
    {
        parentMoved = true;
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

    public void Move(int x, int y)
    {
        if (hasParent && !parentMoved)
        {
            return;
        }

        Direction d = TupleToDirection((x, y));

        Direction relativeDirection = d switch
        {
            Direction.Up => direction,
            Direction.Left => (Direction)(((int)direction + 3) % 4),
            Direction.Down => (Direction)(((int)direction + 2) % 4),
            Direction.Right => (Direction)(((int)direction + 1) % 4),
            _ => direction
        };

        (int newx, int newy) = relativeDirection switch
        {
            Direction.Up => (0, 1),
            Direction.Down => (0, -1),
            Direction.Right => (1, 0),
            Direction.Left => (-1, 0),
            _ => (10, 10)
        };

        direction = relativeDirection;

        if (!ObstacleAt(newx, newy))
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
        }
    }

    private bool ObstacleAt(int x, int y)
    {
        foreach (SceneNode child in children)
        {
            if (child.ObstacleAt(x, y))
            {
                return true;
            }
        }

        float absoluteX = absolutePosition.x;
        float absoluteY = absolutePosition.y;
        float newX = absoluteX + x * moveBy;
        float newY = absoluteY + y * moveBy;

        // lol linear search
        foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag(collisionTag))
        {
            if (
                (Mathf.Abs(obstacle.transform.position.x - newX) < moveBy * .9) &&
                (Mathf.Abs(obstacle.transform.position.z - newY) < moveBy * .9)
            )
            {
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

    // tipPos: is the origin of this scene node
    // topDir: is the y-direction of this node
    public void CompositeTransform(ref Matrix4x4 parentXform, out Vector3 snOrigin, out Vector3 snUp)
    {
        Matrix4x4 orgT = Matrix4x4.Translate(NodeOrigin);
        Matrix4x4 trs = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        
        mCombinedParentXform = parentXform * orgT * trs;
        
        // let's decompose the combined matrix into R, and S
        Vector3 c0 = mCombinedParentXform.GetColumn(0);
        Vector3 c1 = mCombinedParentXform.GetColumn(1);
        Vector3 c2 = mCombinedParentXform.GetColumn(2);
        Vector3 s = new Vector3(c0.magnitude, c1.magnitude, c2.magnitude);
        Quaternion q = Quaternion.LookRotation(c2, c1); // creates a rotation matrix with c2-Forward, c1-up

        snOrigin = mCombinedParentXform.GetColumn(3);
        snUp = c1;

        absolutePosition = new Vector2(snOrigin.x, snOrigin.z);

        float angle = direction switch
        {
            Direction.Up => 0,
            Direction.Right => 90,
            Direction.Down => 180,
            Direction.Left => 270,
            _ => 0
        };

        Quaternion angles = Quaternion.Euler(0, angle, 0);

        q *= angles;

        if (camera != null)
        {

            Quaternion angleAxis = Quaternion.AngleAxis(cameraAngle, cameraAxis);
            camera.transform.localRotation = Quaternion.Euler(angleAxis.eulerAngles.x, angle, angleAxis.eulerAngles.z);
            camera.transform.localPosition = RotatePointAroundPivot(cameraOrigin + snOrigin, snOrigin, angles);
        }

        AxisFrame.transform.localPosition = snOrigin;  // our location is Pivot 
        AxisFrame.transform.localScale = s * kAxisFrameSize;
        AxisFrame.transform.localRotation = q;

        // propagate to all children
        foreach (SceneNode child in children)
        {
            child.CompositeTransform(ref mCombinedParentXform, out snOrigin, out snUp);
        }
        
        // disenminate to primitives
        foreach (NodePrimitive p in PrimitiveList)
        {
            p.SetGrayscale(!hasParent || hasParent && parentMoved);
            p.LoadShaderMatrix(ref mCombinedParentXform);
        }
    }
}