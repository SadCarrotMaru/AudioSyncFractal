using UnityEngine;
using UnityEngine.UI;

public class Plotter : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public AudioSource audioSource;
    private float[] samples = new float[512];

    void Update()
    {
        audioSource.GetOutputData(samples, 0);
        UpdateWaveform();
    }

    void UpdateWaveform()
    {
        Vector3[] points = new Vector3[samples.Length];
        float width = 500f; 
        float height = 200f; 

        for (int i = 0; i < samples.Length; i++)
        {
            float x = Mathf.Lerp(-width / 2, width / 2, (float)i / samples.Length);
            float y = samples[i] * height;
            points[i] = new Vector3(x, y, 0);
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}