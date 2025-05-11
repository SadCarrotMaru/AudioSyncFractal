using UnityEngine;

public class FractalAnimation : MonoBehaviour
{

    public Raymarcher targetRaymarcher;
    public float speed = 1.0f; // Radians per second

    private float currentAngle = 0.0f;
    private const float TwoPI = Mathf.PI * 2f;


    // Animating kaleidoscopic rotation (Scene 3)
    void Update()
    {
        if (targetRaymarcher == null)
            return;

        currentAngle += speed * 0.1f * Time.deltaTime;
        if (currentAngle > TwoPI)
            currentAngle -= TwoPI * 2;

        targetRaymarcher.SetShaderVar(currentAngle);
    }
}

