using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BouncingSphere : MonoBehaviour
{
    [Header("Base Physics Settings")]
    public bool   useGravity      = false;
    public float  initialSpeed    = 5f;    // fallback speed if not using audio
    public bool   constantSpeed   = true;
    public float  cornerJitterDeg = 5f;

    [Header("Audio-Driven Speed")]
    [Tooltip("If true, overrides initialSpeed each FixedUpdate.")]
    public bool      useAudioSpeed        = FractalUIBinder.useAudio;
    [Tooltip("Multiplier applied to the raw audio buffer (0–1) to get your actual speed.")]
    public float     audioSpeedMultiplier = 10f;
    
    [Header("Stuck & Respawn")]
    public float    stuckVelocityThreshold = 0.1f;
    public float    stuckTimeToRespawn     = 1f;
    public Vector3  respawnPoint;            // set from your spawner to cube center

    [Header("Debug Logging")]
    public float    logInterval            = 1f;

    Rigidbody rb;
    float     stuckTimer = 0f;
    float     logTimer   = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Physics.bounceThreshold       = 0f;
        rb.useGravity                 = useGravity;
        initialSpeed = 7f;
        rb.linearDamping                       = 0f;
        rb.angularDamping                = 0f;
        rb.collisionDetectionMode     = CollisionDetectionMode.ContinuousDynamic;
        useAudioSpeed = FractalUIBinder.useAudio; // set from UI
        LaunchRandom(GetCurrentSpeed());
    }

    void FixedUpdate()
    {
        useAudioSpeed = FractalUIBinder.useAudio; // set from UI
        float speedNow = GetCurrentSpeed();

        // enforce exact speed if desired
        if (constantSpeed && rb.linearVelocity.sqrMagnitude > 0.0001f)
            rb.linearVelocity = rb.linearVelocity.normalized * speedNow;

        // debug-log speed occasionally
        logTimer += Time.fixedDeltaTime;
        if (logTimer >= logInterval)
        {
            logTimer = 0f;
        }

        // respawn if stuck
        if (rb.linearVelocity.magnitude < stuckVelocityThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeToRespawn)
            {
                Respawn(speedNow);
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        // average normals
        Vector3 avgN = Vector3.zero;
        foreach (var ct in col.contacts) avgN += ct.normal;
        avgN.Normalize();

        // reflect
        Vector3 dir = Vector3.Reflect(rb.linearVelocity, avgN).normalized;

        float a = Random.Range(-cornerJitterDeg, cornerJitterDeg);
        float b = Random.Range(-cornerJitterDeg, cornerJitterDeg);
        float c = Random.Range(-cornerJitterDeg, cornerJitterDeg);
        dir = Quaternion.Euler(a, b, c) * dir;

        rb.linearVelocity = dir * GetCurrentSpeed();
    }

    float GetCurrentSpeed()
    {
        if (!useAudioSpeed) return initialSpeed;
        var finalSpeed = 100 * AudioPeer.spectralRolloffBuffer;
        return finalSpeed;
    }

    void LaunchRandom(float speed)
    {
        Vector3 d = Random.onUnitSphere;
        if (useGravity) d.y = Mathf.Abs(d.y);
        rb.linearVelocity = d * speed;
        stuckTimer = 0f;
    }

    void Respawn(float speed)
    {
        transform.position = respawnPoint;
        stuckTimer = 0f;
        LaunchRandom(speed);
    }
}
