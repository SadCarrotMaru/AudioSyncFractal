using UnityEngine;

public class SphereSpawner : MonoBehaviour
{
    [Header("Prefab & Container")]
    [Tooltip("Your pre-configured sphere prefab.")]
    public GameObject spherePrefab;

    [Tooltip("The GameObject whose bounds define the spawn volume.")]
    public GameObject containerObject;

    [Header("Spawn Settings")]
    [Tooltip("How many spheres to create.")]
    public int sphereCount = 20;

    void Start()
    {
        if (spherePrefab == null || containerObject == null)
        {
            Debug.LogError("SphereSpawner: assign both Sphere Prefab and Container Object!", this);
            return;
        }

        Bounds bounds;
        var col = containerObject.GetComponent<Collider>();
        if (col != null)
        {
            bounds = col.bounds;
        }
        else
        {
            var rend = containerObject.GetComponent<Renderer>();
            if (rend != null)
                bounds = rend.bounds;
            else
            {
                Debug.LogError("SphereSpawner: containerObject needs a Collider or Renderer to get bounds!", containerObject);
                return;
            }
        }
        
        float marginFrac = 0.25f; 
        Vector3 size      = bounds.size;
        Vector3 innerMin  = bounds.min + size * marginFrac;
        Vector3 innerMax  = bounds.max - size * marginFrac;

        for (int i = 0; i < sphereCount; i++)
        {
            float x = Random.Range(innerMin.x, innerMax.x);
            float y = Random.Range(innerMin.y, innerMax.y);
            float z = Random.Range(innerMin.z, innerMax.z);
            Vector3 spawnPos = new Vector3(x, y, z);
            var go = Instantiate(spherePrefab, spawnPos, Random.rotation);
            var bs = go.GetComponent<BouncingSphere>();
            if (bs != null)
            {
                bs.respawnPoint = bounds.center;
                bs.initialSpeed = Random.Range(1f, 5f);
            }
        }
    }
}