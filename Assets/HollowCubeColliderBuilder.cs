using UnityEngine;

[ExecuteInEditMode]    // so you can rebuild in the Editor
public class HollowCubeColliderBuilder : MonoBehaviour
{
    [Tooltip("Thickness of each inner wall collider, in the same units as your cube scale.")]
    public float wallThickness = 0.1f;

    // rebuild in Edit-mode from the context menu
    [ContextMenu("Rebuild Hollow Walls")]
    void BuildWalls()
    {
        // 1) clear out any old walls
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c.name.StartsWith("Wall_"))
                DestroyImmediate(c.gameObject);
        }

        // 2) get cube half-sizes from its localScale
        // Vector3 s    = transform.localScale;
        Vector3 s = Vector3.one;
        Vector3 half = s * 0.5f;
        float   t    = wallThickness * 0.5f;

        // 3) spawn the six walls as children, in LOCAL space
        CreateWall("Wall_Right",  new Vector3( half.x - t, 0,         0), new Vector3(wallThickness, s.y,          s.z));
        CreateWall("Wall_Left",   new Vector3(-half.x + t, 0,         0), new Vector3(wallThickness, s.y,          s.z));
        CreateWall("Wall_Top",    new Vector3(0,          half.y - t, 0), new Vector3(s.x,           wallThickness, s.z));
        CreateWall("Wall_Bottom", new Vector3(0,         -half.y + t, 0), new Vector3(s.x,           wallThickness, s.z));
        CreateWall("Wall_Front",  new Vector3(0,          0,         half.z - t), new Vector3(s.x,           s.y,          wallThickness));
        CreateWall("Wall_Back",   new Vector3(0,          0,        -half.z + t), new Vector3(s.x,           s.y,          wallThickness));
    }

    void CreateWall(string name, Vector3 localPos, Vector3 localSize)
    {
        var wall = new GameObject(name);
        wall.transform.SetParent(transform, false);       // keep LOCAL space
        wall.transform.localPosition = localPos;
        wall.transform.localRotation = Quaternion.identity;
        wall.transform.localScale    = Vector3.one;       // size baked into collider

        var bc = wall.AddComponent<BoxCollider>();
        bc.size      = localSize;
        bc.isTrigger = false;
    }

    // auto-build when you hit Play
    void Start() => BuildWalls();
}
