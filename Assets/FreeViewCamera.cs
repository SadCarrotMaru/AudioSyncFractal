using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class FreeViewCamera : MonoBehaviour
{
    [Header("Fractal Container")]
    [Tooltip("The fractal or cube GameObject to bounce inside.")]
    public GameObject fractalObject;

    [Header("Base Physics Settings")]
    public bool   useGravity      = false;
    public static float  initialSpeed    = 2f;
    public bool   constantSpeed   = true;
    public float  cornerJitterDeg = 5f;

    [Header("Stuck & Respawn")]
    [Tooltip("If you slow below this, you get teleported back.")]
    public float   stuckVelocityThreshold = 0.1f;
    public float   stuckTimeToRespawn     = 1f;

    Vector3 respawnPoint;
    Rigidbody rb;
    float     stuckTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // physics setup
        rb.useGravity             = useGravity;
        rb.linearDamping                   = 0f;
        rb.angularDamping            = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Start()
    {
        // 1. compute center of your fractal/cube
        if (fractalObject != null)
        {
            Bounds b;
            var col = fractalObject.GetComponent<Collider>();
            if (col != null) b = col.bounds;
            else
            {
                var rend = fractalObject.GetComponent<Renderer>();
                if (rend != null) b = rend.bounds;
                else
                {
                    Debug.LogError("AutoBounceCamera: fractalObject needs a Collider or Renderer.", fractalObject);
                    return;
                }
            }
            respawnPoint = b.center;
        }
        else
        {
            Debug.LogError("AutoBounceCamera: assign fractalObject!", this);
            respawnPoint = transform.position;
        }

        // 2. place at center
        transform.position = respawnPoint;

        // 3. launch off in a random direction
        float speed = initialSpeed;
        Vector3 dir = Random.onUnitSphere;
        rb.linearVelocity  = dir.normalized * speed;
    }

    void FixedUpdate()
    {
        // keep exact speed if requested
        if (constantSpeed && rb.linearVelocity.sqrMagnitude > 0.0001f)
            rb.linearVelocity = rb.linearVelocity.normalized * initialSpeed;

        // stuck detection
        if (rb.linearVelocity.magnitude < stuckVelocityThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeToRespawn)
                Respawn();
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        // average contact normals (for corners)
        Vector3 avgN = Vector3.zero;
        foreach (var ct in col.contacts) avgN += ct.normal;
        avgN.Normalize();

        // reflect & jitter
        Vector3 dir = Vector3.Reflect(rb.linearVelocity, avgN).normalized;
        float a = Random.Range(-cornerJitterDeg, cornerJitterDeg);
        float b = Random.Range(-cornerJitterDeg, cornerJitterDeg);
        float c = Random.Range(-cornerJitterDeg, cornerJitterDeg);
        dir = Quaternion.Euler(a, b, c) * dir;

        // re-launch
        rb.linearVelocity = dir * initialSpeed;
    }

    void Respawn()
    {
        // teleport back to center + launch
        transform.position = respawnPoint;
        stuckTimer = 0f;
        Vector3 dir = Random.onUnitSphere;
        rb.linearVelocity = dir.normalized * initialSpeed;
    }
}
