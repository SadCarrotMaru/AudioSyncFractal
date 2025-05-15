using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Sentis;

public class MoodInference : MonoBehaviour
{
    // Scaler means (from StandardScaler.mean_)
    private static readonly float[] _means = new float[]
    {
        1.04637900e+00f,
        -4.50532199e-02f,
        1.45855158e+02f,
        2.06780533e+03f,
        2.83933552e+03f,
        0.00000000e+00f,
        4.24325856e+03f
    };

// Scaler scales (from StandardScaler.scale_)
    private static readonly float[] _scales = new float[]
    {
        1.74297914e-01f,
        1.98088347e-02f,
        3.76800229e+01f,
        3.84109961e+02f,
        3.75783130e+02f,
        1.00000000e+00f,
        9.41163460e+02f
    };
    
    private static readonly float valenceMean = 4.674240362811791f;
    private static readonly float valenceStd = 1.3726228795146682f;
    private static readonly float arousalMean = 4.934036281179138f;
    private static readonly float arousalStd = 1.284108299931673f;
        
    [Tooltip("Your .onnx model imported under Assets/Resources (no extension)")]
    public string modelResourcePath = "mood_model_reduced";

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
        Debug.Log($"Old arousal: {arousal}, valence: {valence}");

        arousal = Mathf.Clamp01((arousal - arousalStd) / arousalMean);
        valence = Mathf.Clamp01((valence - valenceStd) / valenceMean);
        
        Debug.Log($"New arousal: {arousal}, valence: {valence}");
        // Hue: 0.6 → 0.0 (blue → red) as valence goes from 0 → 1
        float hue = 0.6f * (1f - valence);
        // Saturation = arousal (0 = muted, 1 = vivid)
        float sat = arousal;
        // Value = 1 for full brightness
        float val = 1f;
        return Color.HSVToRGB(hue, sat, val);
    }

    // public Task<float[]> PredictMoodAsync(float[] features)
    // {
    //     return Task.Run(() =>
    //     {
    //         // 1) Build your [1×N] input tensor
    //         var tensor = new Tensor<float>(new TensorShape(1, features.Length));
    //         for (int i = 0; i < features.Length; i++)
    //             tensor[0, i] = features[i];
    //
    //         // 2) Synchronously execute on this thread-pool thread
    //         worker.Schedule(tensor);
    //
    //         // 3) Immediately read out the finished output
    //         using var output = worker.PeekOutput() as Tensor<float>;
    //         int count = output.shape[1];
    //         var result = new float[count];
    //         for (int i = 0; i < count; i++)
    //             result[i] = output[0, i];
    //
    //         return result;
    //     });
    // }

    public async Task<float[]> PredictMoodAsync(float[] monoSamples, int sampleRate)
    {
         var normalizedFeatures = await Task.Run(async () =>
        {
            // 1) Extract features (heavy computation, run in background thread)
            var feats = AudioFeatureExtractorNWaves.Extract(monoSamples, sampleRate);

            return new float[7]
            {
                (feats.MfccMean - _means[0]) / _scales[0],
                (feats.LpcMean - _means[1]) / _scales[1],
                (feats.PitchMean - _means[2]) / _scales[2],
                (feats.CentroidMean - _means[3]) / _scales[3],
                (feats.BandwidthMean - _means[4]) / _scales[4],
                (feats.ContrastMean - _means[5]) / _scales[5],
                (feats.RolloffMean - _means[6]) / _scales[6]
            };
        }); 

        // 2) Create input tensor
        using var inputTensor = new Tensor<float>(new TensorShape(1, 7));
        for (int i = 0; i < 7; i++)
            inputTensor[0, i] = normalizedFeatures[i];

        // 3) Schedule inference (Sentis)
        worker.Schedule(inputTensor);
        var outputTensor = worker.PeekOutput() as Tensor<float>;

        // 4) Await GPU→CPU readback asynchronously
        using var cpuTensor = await outputTensor.ReadbackAndCloneAsync();

        // 5) Retrieve output
        float[] result = new float[cpuTensor.shape[1]];
        for (int i = 0; i < result.Length; i++)
            result[i] = cpuTensor[0, i];

        return result;
    }


    /// <summary>
    /// Runs your 9‐dim feature vector through the model.
    /// Returns float[2] = { valence, arousal }.
    /// </summary>
    public float[] PredictMood(float[] features)
    {
        Debug.Log($"Features: [{string.Join(", ", features)}]");
        for (int i = 0; i < 7; i++)
        {
            features[i] = (features[i] - _means[i]) / _scales[i];
        }
        Debug.Log($"Normalized Features: [{string.Join(", ", features)}]");
        // sanity check
        if (features.Length != 7)
            Debug.LogWarning("Expecting 7 features, got " + features.Length);

        // 3) Create a Tensor<float> of shape [1,9]
        var tensor = new Tensor<float>(new TensorShape(1, features.Length));
        foreach (var i in Enumerable.Range(0, features.Length))
            tensor[0, i] = features[i];
        
        worker.Schedule(tensor);
        var outputTensor = worker.PeekOutput() as Tensor<float>;
        //
        // bool inferencePending = true;
        // var awaiter = outputTensor.ReadbackAndCloneAsync().GetAwaiter();
        // awaiter.OnCompleted(() =>
        // {
        //     var tensorOut = awaiter.GetResult();
        //     inferencePending = false;
        //     tensorOut.Dispose();
        // });
        //
        
        var cpuTensor = outputTensor.ReadbackAndClone();
        
        int outCount = cpuTensor.shape[1];
        var result = new float[outCount];
        for (int i = 0; i < outCount; i++)
            result[i] = cpuTensor[0, i];

        return result;
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}