using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Sentis;

public class MoodInference : MonoBehaviour
{
    [Tooltip("Your .onnx model imported under Assets/Resources (no extension)")]
    public string modelResourcePath = "mood_model_pytorch";

    Model runtimeModel;
    Worker worker;

    void Awake()
    {
        // 1) Load the ONNX from Resources/<modelResourcePath>.onnx
        var modelAsset = Resources.Load<ModelAsset>(modelResourcePath);
        if (modelAsset == null)
        {
            Debug.LogError($"Failed to load ModelAsset at Resources/{modelResourcePath}.onnx");
            enabled = false;
            return;
        }

        // 2) Build the runtime model & worker
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.CPU);
    }
    
    /// <summary>
    /// Convert valence/arousal (both 0–1) → a Unity Color (HSV).
    /// </summary>
    public static Color FromValenceArousal(float valence, float arousal)
    {
        // Hue: 0.6 → 0.0 (blue → red) as valence goes from 0 → 1
        float hue = 0.6f * (1f - Mathf.Clamp01(valence));
        // Saturation = arousal (0 = muted, 1 = vivid)
        float sat = Mathf.Clamp01(arousal);
        // Value = 1 for full brightness
        float val = 1f;
        return Color.HSVToRGB(hue, sat, val);
    }

    public Task<float[]> PredictMoodAsync(float[] features)
    {
        return Task.Run(() =>
        {
            // 1) Build the input tensor
            var tensor = new Tensor<float>(new TensorShape(1, features.Length));
            for (int i = 0; i < features.Length; i++)
                tensor[0, i] = features[i];

            // 2) Kick off the async schedule
            var scheduleEnum = worker.ScheduleIterable(tensor);

            // 3) Drive it to completion on this thread
            while(scheduleEnum.MoveNext())
            {
                // Optionally yield control so other tasks can run:
                Thread.Yield();
            }

            // 4) Now it’s done—peek the result
            using var output = worker.PeekOutput() as Tensor<float>;
            int count = output.shape[1];
            var result = new float[count];
            for (int i = 0; i < count; i++)
                result[i] = output[0, i];

            return result;
        });
    }

    /// <summary>
    /// Runs your 10‐dim feature vector through the model.
    /// Returns float[2] = { valence, arousal }.
    /// </summary>
    public float[] PredictMood(float[] features)
    {
        // sanity check
        if (features.Length != 9)
            Debug.LogWarning("Expecting 10 features, got " + features.Length);

        // 3) Create a Tensor<float> of shape [1,9]
        var tensor = new Tensor<float>(new TensorShape(1, features.Length));
        foreach (var i in Enumerable.Range(0, features.Length))
            tensor[0, i] = features[i];
        
        worker.Schedule(tensor);

        // 3) read the output
        using var output = worker.PeekOutput() as Tensor<float>;
        int outCount = output.shape[1];
        var result = new float[outCount];
        for (int i = 0; i < outCount; i++)
            result[i] = output[0, i];

        return result;
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}