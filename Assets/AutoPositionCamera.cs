using UnityEngine;

[ExecuteInEditMode]
public class AutoPositionCamera : MonoBehaviour
{
    [Tooltip("The GameObject to frame.")]
    public GameObject targetObject;

    [Tooltip("Direction from which to view (defaults to diagonal).")]
    public Vector3 viewDirection = new Vector3(1f, 1f, 1f);

    [Tooltip("Distance multiplier relative to the target's max extent.")]
    public float distanceMultiplier = 2f;

    [Tooltip("Up direction for camera orientation.")]
    public Vector3 upDirection = Vector3.up;

    void Start()
    {
        PositionCamera();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        PositionCamera();
    }
#endif

    /// <summary>
    /// Positions this camera at a vantage point looking toward the target's center.
    /// </summary>
    public void PositionCamera()
    {
        if (targetObject == null) return;

        // Obtain world-space bounds
        Bounds bounds;
        var rend = targetObject.GetComponent<Renderer>();
        if (rend != null)
        {
            bounds = rend.bounds;
        }
        else
        {
            var col = targetObject.GetComponent<Collider>();
            if (col != null)
                bounds = col.bounds;
            else
                return;
        }

        Vector3 center = bounds.center;
        float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        Vector3 dirNorm = viewDirection.normalized;

        // Place the camera at center + (direction * distance)
        transform.position = center + dirNorm * maxExtent * distanceMultiplier;
        transform.rotation = Quaternion.LookRotation(center - transform.position, upDirection);
    }
}