using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions.ColorPicker; 
using UnityEngine.Networking;                
using SFB;                                  

public class FractalUIBinder : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown         sceneDropdown;
    public Slider               speedSlider;
    public ColorPickerControl   baseColorPicker;
    public ColorPickerControl   glowColorPicker;
    public Button               browseButton; 
    public TMP_Text             songLabel; 
    
    [Header("Fractal & Audio")]
    public FractalAnimation     fractalAnim;
    public AudioPeer            audioPeer; 
    public MoodInference        moodModel;
    public static bool useAudio = false; // static for other scripts to access
    AudioSource audioSource;             
    void Start()
    {
        useAudio = false; // reset static variable
        var names = System.Enum.GetNames(typeof(FractalScene));
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(new List<string>(names));
        sceneDropdown.value = (int)fractalAnim.scene - 1;
        sceneDropdown.RefreshShownValue();
        sceneDropdown.onValueChanged.AddListener(OnSceneChanged);

        speedSlider.minValue = 0f;
        speedSlider.maxValue = 5f;
        speedSlider.value    = fractalAnim.speed;
        speedSlider.onValueChanged.AddListener(v =>
        {
            fractalAnim.speed = v;
            FreeViewCamera.initialSpeed = v;
        });

        baseColorPicker.CurrentColor = fractalAnim.color;
        baseColorPicker.onValueChanged.AddListener(c =>
        {
            fractalAnim.color = c;
            fractalAnim.targetRaymarcher.SetShaderColor(c);
        });

        glowColorPicker.CurrentColor = fractalAnim.glow_color;
        glowColorPicker.onValueChanged.AddListener(c =>
        {
            fractalAnim.glow_color = c;
            fractalAnim.targetRaymarcher.SetShaderGlow(c);
        });

        browseButton.onClick.AddListener(OpenMp3Browser);
        songLabel.text = "Song: None";
        fractalAnim.useAudio = false;

        fractalAnim.audioType = AudioType.SpectralRollOff;
    }

    private void OnSceneChanged(int idx)
    {
        fractalAnim.scene = (FractalScene)(idx + 1);
        fractalAnim.targetRaymarcher.SetShaderScene((float)fractalAnim.scene);
        sceneDropdown.RefreshShownValue();
    }

    void OpenMp3Browser()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            title:      "Select MP3",
            directory:  "",
            extension:  "mp3",
            multiselect:false
        );

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            StartCoroutine(LoadAndPlayMp3(paths[0]));
    }
    
    // IEnumerator LoadAndPlayMp3(string fullPath)
    // {
    //     var url = "file:///" + fullPath.Replace("\\", "/");
    //     using (var uwr = UnityWebRequestMultimedia.GetAudioClip(url, UnityEngine.AudioType.MPEG))
    //     {
    //         yield return uwr.SendWebRequest();
    //         if (uwr.result != UnityWebRequest.Result.Success)
    //         {
    //             Debug.LogError($"MP3 load error: {uwr.error}");
    //             songLabel.text = "Song: <error>";
    //         }
    //         else
    //         {
    //             var clip = DownloadHandlerAudioClip.GetContent(uwr);
    //             audioPeer.PlayClip(clip);
    //             fractalAnim.useAudio = true;
    //             useAudio = true;
    //             songLabel.text = "Song: " + Path.GetFileNameWithoutExtension(fullPath);
    //
    //             int samplesCount = clip.samples * clip.channels;
    //             float[] raw = new float[samplesCount];
    //             clip.GetData(raw, 0);
    //
    //             float[] mono = new float[clip.samples];
    //             if (clip.channels == 1)
    //             {
    //                 mono = raw;
    //             }
    //             else
    //             {
    //                 for (int i = 0; i < clip.samples; i++)
    //                 {
    //                     float sum = 0;
    //                     for (int c = 0; c < clip.channels; c++)
    //                         sum += raw[i * clip.channels + c];
    //                     mono[i] = sum / clip.channels;
    //                 }
    //             }
    //
    //             float[] monoSamples = mono;
    //             int sampleRate = clip.frequency;
    //
    //             // Feature extraction
    //             var feats = AudioFeatureExtractorNWaves.Extract(monoSamples, sampleRate);
    //             float[] input = new float[7] {
    //                 feats.MfccMean,
    //                 feats.LpcMean,
    //                 feats.PitchMean,
    //                 feats.CentroidMean,
    //                 feats.BandwidthMean,
    //                 feats.ContrastMean,
    //                 feats.RolloffMean
    //             };
    //
    //             // --- ⚡ Non-blocking prediction ⚡ ---
    //             var moodTask = moodModel.PredictMoodAsync(input);
    //
    //             // Wait asynchronously without blocking the main thread
    //             yield return new WaitUntil(() => moodTask.IsCompleted);
    //
    //             if (moodTask.Exception != null)
    //             {
    //                 Debug.LogError("Mood prediction failed: " + moodTask.Exception);
    //             }
    //             else
    //             {
    //                 var moodResult = moodTask.Result; // Now safe, task complete
    //                 Color suggested = MoodInference.FromValenceArousal(moodResult[0], moodResult[1]);
    //
    //                 Debug.Log($"Valence: {moodResult[0]}, Arousal: {moodResult[1]}, Suggested Color: {suggested}");
    //
    //                 // Update color pickers safely
    //                 baseColorPicker.CurrentColor = suggested;
    //                 baseColorPicker.onValueChanged.Invoke(suggested);
    //                 glowColorPicker.CurrentColor = suggested;
    //                 glowColorPicker.onValueChanged.Invoke(suggested);
    //             }
    //         }
    //     }
    // }

    IEnumerator LoadAndPlayMp3(string fullPath)
{
    var url = "file:///" + fullPath.Replace("\\", "/");
    using (var uwr = UnityWebRequestMultimedia.GetAudioClip(url, UnityEngine.AudioType.MPEG))
    {
        yield return uwr.SendWebRequest();
        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"MP3 load error: {uwr.error}");
            songLabel.text = "Song: <error>";
        }
        else
        {
            var clip = DownloadHandlerAudioClip.GetContent(uwr);
            audioPeer.PlayClip(clip);
            fractalAnim.useAudio = true;
            useAudio = true;
            songLabel.text = "Song: " + Path.GetFileNameWithoutExtension(fullPath);

            int samplesCount = clip.samples * clip.channels;
            float[] raw = new float[samplesCount];
            clip.GetData(raw, 0);

            float[] mono = clip.channels == 1 ? raw : new float[clip.samples];
            if (clip.channels > 1)
            {
                for (int i = 0; i < clip.samples; i++)
                {
                    float sum = 0;
                    for (int c = 0; c < clip.channels; c++)
                        sum += raw[i * clip.channels + c];
                    mono[i] = sum / clip.channels;
                }
            }

            // FULLY OFFLOADED TO BACKGROUND THREAD
            var moodTask = moodModel.PredictMoodAsync(mono, clip.frequency);
            yield return new WaitUntil(() => moodTask.IsCompleted);

            if (moodTask.Exception != null)
                Debug.LogError("Mood prediction failed: " + moodTask.Exception);
            else
            {
                var mood = moodTask.Result;
                Color suggested = MoodInference.FromValenceArousal(mood[0], mood[1]);

                baseColorPicker.CurrentColor = suggested;
                baseColorPicker.onValueChanged.Invoke(suggested);
                glowColorPicker.CurrentColor = suggested;
                glowColorPicker.onValueChanged.Invoke(suggested);
            }
        }
    }
}

    // IEnumerator LoadAndPlayMp3(string fullPath)
    // {
    //     var url = "file:///" + fullPath.Replace("\\", "/");
    //     using (var uwr = UnityWebRequestMultimedia.GetAudioClip(url, UnityEngine.AudioType.MPEG))        {
    //         yield return uwr.SendWebRequest();
    //         if (uwr.result != UnityWebRequest.Result.Success)
    //         {
    //             Debug.LogError($"MP3 load error: {uwr.error}");
    //             songLabel.text = "Song: <error>";
    //         }
    //         else
    //         {
    //             var clip = DownloadHandlerAudioClip.GetContent(uwr);
    //             audioPeer.PlayClip(clip);
    //             fractalAnim.useAudio = true;
    //             useAudio = true;
    //             songLabel.text = "Song: " + Path.GetFileNameWithoutExtension(fullPath);
    //
    //             // 2) pull out the raw samples (mono)
    //             int   samplesCount = clip.samples * clip.channels;
    //             float[] raw = new float[samplesCount];
    //             clip.GetData(raw, 0);
    //
    //             // if stereo, downmix to mono:
    //             float[] mono = new float[clip.samples];
    //             if (clip.channels == 1)
    //             {
    //                 mono = raw;
    //             }
    //             else
    //             {
    //                 for (int i = 0; i < clip.samples; i++)
    //                 {
    //                     // average across channels
    //                     float sum = 0;
    //                     for (int c = 0; c < clip.channels; c++)
    //                         sum += raw[i * clip.channels + c];
    //                     mono[i] = sum / clip.channels;
    //                 }
    //             }
    //
    //             float[] monoSamples = mono; 
    //             int     sampleRate  = clip.frequency;
    //             
    //             var feats = AudioFeatureExtractorNWaves.Extract(monoSamples, sampleRate);
    //             // 1) extract features
    //
    //             // 2) build the input vector
    //             float[] input = new float[7] {
    //                 feats.MfccMean,
    //                 feats.LpcMean,
    //                 feats.PitchMean,
    //                 feats.CentroidMean,
    //                 feats.BandwidthMean,
    //                 feats.ContrastMean,
    //                 feats.RolloffMean
    //             };
    //
    //             var moodTask = moodModel.PredictMood(input);
    //             Color suggested = MoodInference.FromValenceArousal(moodTask[0], moodTask[1]);
    //             Debug.Log($"Valence: {moodTask[0]}, Arousal: {moodTask[1]}, Color: {suggested}");
    //                 
    //             baseColorPicker.CurrentColor = suggested;
    //             baseColorPicker.onValueChanged.Invoke(suggested);
    //             glowColorPicker.CurrentColor = suggested;
    //             glowColorPicker.onValueChanged.Invoke(suggested);
    //         }
    //     }
    // }
}
