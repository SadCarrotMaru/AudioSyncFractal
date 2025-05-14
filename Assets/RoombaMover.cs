// using UnityEngine;
//
// [RequireComponent(typeof(Rigidbody), typeof(Collider))]
// public class RoombaMover : MonoBehaviour
// {
//     [Header("Movement Settings")]
//     public float speed = 5f;
//     public float jitterAngle = 30f;
//
//     [Header("Spawn Bounds")]
//     [Tooltip("Drag in the BoxCollider on your fractal cube here")]
//     public BoxCollider boundaryCollider;
//
//     private Rigidbody _rb;
//     Vector3   _dir;
//     SphereCollider _sphereCol;
//
//     void Start()
//     {
//         if (boundaryCollider == null)
//         {
//             Debug.LogError("RoombaMover: please assign a BoxCollider in the Inspector!");
//             enabled = false;
//             return;
//         }
//
//         // 1) Rigidbody setup
//         _rb = GetComponent<Rigidbody>();
//         _rb.useGravity = false;
//         _rb.constraints = RigidbodyConstraints.FreezeRotation;
//         _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
//
//         // 2) Pick a random starting point INSIDE the fractal cube
//         Bounds b = boundaryCollider.bounds;
//         
//         Debug.Log($"{b.min.x}, {b.min.y}, {b.min.z}");
//         Debug.Log($"{b.max.x}, {b.max.y}, {b.max.z}");
//         transform.position = new Vector3(
//             Random.Range(b.min.x / 2, b.max.x / 2),
//             Random.Range(b.min.y / 2, b.max.y / 2),
//             Random.Range(b.min.z / 2, b.max.z / 2)
//         );
//
//         // 3) Pick an initial horizontal direction
//         _dir = Random.onUnitSphere;
//         _dir.y = 0;
//         _dir.Normalize();
//         
//         _sphereCol = GetComponent<SphereCollider>();
//         // 4) Kick off the motion
//         _rb.linearVelocity = _dir * speed;
//     }
//
//     void FixedUpdate()
//     {
//         // maintain constant speed in this direction
//         _rb.linearVelocity = _dir * speed;
//     }
//
//     void OnCollisionEnter(Collision col)
//     {
//         Debug.Log($"{col.gameObject.name} hit {gameObject.name}" +
//             $" at {col.contacts[0].point} with normal {col.contacts[0].normal}" +
//             $" and relative velocity {col.relativeVelocity}" +
//             $" and speed {_rb.linearVelocity.magnitude}");
//     
//         Debug.Log($"Before we were {_dir} and speed {speed}");
//         // reflect off the surface normal
//         Vector3 normal = col.contacts[0].normal;
//         _dir = Vector3.Reflect(_dir, normal);
//
//         // jitter by rotating around a random axis
//         Vector3 jitterAxis = Random.onUnitSphere;
//         float angle = Random.Range(-jitterAngle, jitterAngle);
//         _dir = Quaternion.AngleAxis(angle, jitterAxis) * _dir;
//
//         // renormalize just in case
//         _dir.Normalize();
//
//         // snap inside so we don't tunnel
//         float r = _sphereCol.radius * transform.lossyScale.x;
//         transform.position = col.contacts[0].point + normal * (r + 0.001f);
//         
//         Debug.Log($"Now we are {_dir} and speed {speed} at {transform.position}" +
//             $" with radius {r} and scale {transform.lossyScale.x} and position {transform.position}");
//         // reapply velocity
//         _rb.linearVelocity = _dir * speed;
//     }
// }

using UnityEngine;
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class RoombaMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;

    [Header("Spawn Bounds")]
    [Tooltip("Leave empty to auto-find the GameObject named \"Fractal\"")]
    public GameObject fractalObject;

    Rigidbody    _rb;
    BoxCollider  _boundary;
    Vector3      _center;
    Vector3      _halfExtents;
    Vector3      _dir;

    void Start()
    {
        // 1) Find your cube if you didn’t drag it in
        if (fractalObject == null)
            fractalObject = GameObject.Find("Fractal");

        // 2) Grab its BoxCollider
        _boundary = fractalObject.GetComponent<BoxCollider>();
        if (_boundary == null)
        {
            Debug.LogError("RoombaMover: couldn't find a BoxCollider on your Fractal object!");
            enabled = false;
            return;
        }

        // 3) Cache the cube's center & half-size
        var b = _boundary.bounds;
        _center      = b.center;
        _halfExtents = b.extents;

        // 4) Rigidbody setup
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // 5) Spawn somewhere inside the cube
        transform.position = _center;

        // 6) Aim directly at the cube’s center
        _dir = (_center - transform.position).normalized;
        _rb.linearVelocity = _dir * speed;      // ← use .velocity, not .linearVelocity
    }

    void FixedUpdate()
    {
        // 1) Keep driving toward the center
        _rb.linearVelocity = _dir * speed;

        // 2) If we somehow got outside, flip direction
        Vector3 p = transform.position;
        // Debug.Log($"p: {p} center: {_center} halfExtents: {_halfExtents}");
        if (p.x < _center.x - _halfExtents.x ||
            p.x > _center.x + _halfExtents.x ||
            p.y < _center.y - _halfExtents.y ||
            p.y > _center.y + _halfExtents.y ||
            p.z < _center.z - _halfExtents.z ||
            p.z > _center.z + _halfExtents.z)
        {
            _dir = -_dir;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log($"{col.gameObject.name} hit {gameObject.name}" +
            $" at {col.contacts[0].point} with normal {col.contacts[0].normal}" +
            $" and relative velocity {col.relativeVelocity}" +
            $" and speed {_rb.linearVelocity.magnitude}");
        // as a backup, if we collide with the cube, simply reverse
        if (col.collider == _boundary)
        {
            _dir = -_dir;
        }
    }
}