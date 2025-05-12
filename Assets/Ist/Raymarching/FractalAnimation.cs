using UnityEngine;

public enum FractalScene
{
    Kaleidoscopic = 1,
    Tglad = 2,
    Hartverdrahtet = 3,
    Kleinian = 4,
    Knightyan = 5
}

public class FractalAnimation : MonoBehaviour
{

    public Raymarcher targetRaymarcher;
    public FractalScene scene = FractalScene.Kaleidoscopic; // Which fractal to render
    [Range(0f, 5f)]
    public float speed = 1f; // var increase per second
    private float currentVar = 0.0f;
    // private const float TwoPI = Mathf.PI * 2f;

    // Set the fractal type
    void Start()
    {
        if (targetRaymarcher == null)
            return;
        targetRaymarcher.SetShaderScene((float)scene);
    }

    // Animating kaleidoscopic rotation (Scene 1)
    void Update()
    {
        if (targetRaymarcher == null)
            return;

        targetRaymarcher.SetShaderScene((float)scene);

        currentVar += speed * Time.deltaTime;

        targetRaymarcher.SetShaderVar(currentVar);
    }
}

