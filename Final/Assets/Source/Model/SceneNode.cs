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
    private bool parentMoved = true;

    private Vector2Int absolutePosition = Vector2Int.zero;

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
                childNode.HasParent();
            }
        }
    }

	// Use this for initialization
	protected void Start () {
        // Debug.Log("PrimitiveList:" + PrimitiveList.Count);
	}

    private void HasParent()
    {
        hasParent = true;
    }

    private void ParentMoved()
    {
        parentMoved = true;
    }

    public KeyCode GetControl(InputManager.Key c)
    {
        return upDownLeftRight[(int)c];
    }

    public void Move(int x, int y)
    {
        if (hasParent && !parentMoved)
        {
            return;
        }
        //Debug.Log(absoluteX + ", " + absoluteY);

        if (!CheckCanMove(x, y))
        {
            /*
            (int newx, int newy) = direction switch
            {
                Direction.Up => (x, y),
                Direction.Down => (x, -y),
                Direction.Right => (y, x),
                Direction.Left => (y, -x),
                _ => (x, y)
            };
            direction = (newx, newy) switch
            {
                (0, 1) => direction,
                (0, -1) => (Direction)(((int)direction + 2) % 4),
                (1, 0) => (Direction)(((int)direction + 1) % 4),
                (-1, 0) => (Direction)(((int)direction - 1) % 4),
                _ => direction
            };
            */
            NodeOrigin = new Vector3(
                NodeOrigin.x + x * moveBy / moveBy,
                NodeOrigin.y,
                NodeOrigin.z + y * moveBy / moveBy
            );
            //Debug.Log(hasParent);
            parentMoved = false;
            foreach (SceneNode child in children)
            {
                child.ParentMoved();
            }
        }
    }

    private bool CheckCanMove(int x, int y)
    {
        int absoluteX = absolutePosition.x;
        int absoluteY = absolutePosition.y;
        int newX = absoluteX + x * moveBy;
        int newY = absoluteY + y * moveBy;

        // lol linear search
        foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag(collisionTag))
        {
            if (
                (Mathf.Abs(obstacle.transform.position.x - newX) < Mathf.Epsilon) &&
                (Mathf.Abs(obstacle.transform.position.y - newY) < Mathf.Epsilon)
            )
            {
                Debug.Log("obs " + (obstacle.transform.position.x, obstacle.transform.position.y));
                return true;
            }
        }

        foreach (SceneNode child in children)
        {
            if (child.CheckCanMove(x, y))
            {
                return true;
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

        if (camera != null)
        {
            //Debug.Log("cam" + q.eulerAngles);
            camera.transform.localRotation = Quaternion.AngleAxis(cameraAngle, cameraAxis) * q;
            camera.transform.localPosition = cameraOrigin + snOrigin;
        }

        q *= direction switch
        {
            Direction.Up => Quaternion.identity,
            Direction.Right => Quaternion.Euler(0, 90, 0),
            Direction.Down => Quaternion.Euler(0, 180, 0),
            Direction.Left => Quaternion.Euler(0, 270, 0),
            _ => Quaternion.identity
        };

        AxisFrame.transform.localPosition = snOrigin;  // our location is Pivot 
        AxisFrame.transform.localScale = s * kAxisFrameSize;
        AxisFrame.transform.localRotation = q;

        absolutePosition = new Vector2Int((int)snOrigin.x, (int)snOrigin.z);

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