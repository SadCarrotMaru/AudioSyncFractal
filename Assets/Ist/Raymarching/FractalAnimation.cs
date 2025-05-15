using Unity.Mathematics.Geometry;
using UnityEngine;

public enum FractalScene
{
    Kaleidoscopic = 1,
    Tglad = 2,
    Hartverdrahtet = 3,
    Kleinian = 4,
    Knightyan = 5,
}

public enum AudioType
{ 
    SpectralRollOff = 1,
    Amplitude = 2
}

public class FractalAnimation : MonoBehaviour
{
    public bool useAudio = false;
    // public AudioType audioType = AudioType.Amplitude;
    public AudioType audioType = AudioType.SpectralRollOff;

    public Raymarcher targetRaymarcher;
    public FractalScene scene = FractalScene.Hartverdrahtet; // Which fractal to render
    [Range(0f, 5f)]
    public float speed = 1f; // var increase per second
    [ColorUsage(true, true)]
    public Color color = Color.white;
    [ColorUsage(true, true)]
    public Color glow_color = Color.white;
    private float currentVar = 0.0f;
    // private const float TwoPI = Mathf.PI * 2f;

    [Header("Smoothing Settings")]
    [Range(0.01f, 1f)]
    public float colorSmoothTime = 0.1f; 
    [Range(0.01f, 1f)]
    public float varSmoothTime = 0.1f;

    private float smoothedVar;
    private float colorVelocity;
    private float varVelocity;
    private float smoothedIntensity = 1f;


    // Set the fractal type
    void Start()
    {
        if (targetRaymarcher == null)
            return;
        targetRaymarcher.SetShaderScene((float)scene);
        targetRaymarcher.SetShaderColor(color);
        targetRaymarcher.SetShaderGlow(glow_color);
    }

    void Update()
    {
        if (targetRaymarcher == null)
            return;

        var intensity = Mathf.PingPong(AudioPeer.spectralRolloffBuffer, 1.0f);
        smoothedIntensity = Mathf.SmoothDamp(smoothedIntensity, intensity,
                                            ref colorVelocity, colorSmoothTime);

        Color dynamicColor = color;
        
        if (useAudio)
        {
            float scaled = Mathf.Clamp01(intensity * 5f);
            dynamicColor = new Color(
                color.r * scaled,
                color.g * scaled,
                color.b * scaled,
                color.a
            );
        }
        
        
        targetRaymarcher.SetShaderScene((float)scene);
        targetRaymarcher.SetShaderColor(dynamicColor);
        targetRaymarcher.SetShaderGlow(glow_color);

        currentVar += speed * Time.deltaTime;

        if (useAudio && audioType == AudioType.Amplitude)
            currentVar = AudioPeer.amplitudeBuffer * 10.0f;
        else if (useAudio && audioType == AudioType.SpectralRollOff)
            currentVar = AudioPeer.spectralRolloffBuffer * 10.0f;

        smoothedVar = Mathf.SmoothDamp(smoothedVar, currentVar,
                                     ref varVelocity, varSmoothTime);


        targetRaymarcher.SetShaderVar(smoothedVar);
    }
}

