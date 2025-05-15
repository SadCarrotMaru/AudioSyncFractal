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

    IEnumerator LoadAndPlayMp3(string fullPath)
    {
        var url = "file:///" + fullPath.Replace("\\", "/");
        using (var uwr = UnityWebRequestMultimedia.GetAudioClip(url, UnityEngine.AudioType.MPEG))        {
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

                // 2) pull out the raw samples (mono)
                int   samplesCount = clip.samples * clip.channels;
                float[] raw = new float[samplesCount];
                clip.GetData(raw, 0);

                // if stereo, downmix to mono:
                float[] mono = new float[clip.samples];
                if (clip.channels == 1)
                {
                    mono = raw;
                }
                else
                {
                    for (int i = 0; i < clip.samples; i++)
                    {
                        // average across channels
                        float sum = 0;
                        for (int c = 0; c < clip.channels; c++)
                            sum += raw[i * clip.channels + c];
                        mono[i] = sum / clip.channels;
                    }
                }

                float[] monoSamples = mono; 
                int     sampleRate  = clip.frequency;

                // Kick off the heavy work on a ThreadPool thread:
                var moodTask = Task.Run(() =>
                {
                    // 1) extract features
                    var feats = AudioFeatureExtractorNWaves.Extract(monoSamples, sampleRate);

                    // 2) build the input vector
                    float[] input = new float[9] {
                        feats.MfccMean,
                        feats.ChromaMean,
                        feats.MelspecMean,
                        feats.LpcMean,
                        feats.PitchMean,
                        feats.CentroidMean,
                        feats.BandwidthMean,
                        feats.ContrastMean,
                        feats.RolloffMean
                    };
                
                    Debug.Log($"Before mood inference: {input[0]:F2} {input[1]:F2} {input[2]:F2} {input[3]:F2} {input[4]:F2} {input[5]:F2} {input[6]:F2} {input[7]:F2} {input[8]:F2}");
                    // 3) run Sentis inference (this is thread-safe as long as you don’t touch UnityEngine objects here)
                    float[] mood = moodModel.PredictMood(input);
                    
                    Debug.Log($"Mood inference: {mood[0]:F2} valence, {mood[1]:F2} arousal");
                    return mood; // [valence, arousal]
                });

                // Meanwhile the coroutine yields until the Task completes:
                while (!moodTask.IsCompleted)
                    yield return null;

                if (moodTask.IsFaulted)
                {
                    Debug.LogError($"Mood inference failed: {moodTask.Exception}");
                    yield break;
                }

                var moodResult = moodTask.Result;
                float valence = moodResult[0];
                float arousal = moodResult[1];

                // 4) back on Unity thread—build your color and apply:
                float h = Mathf.Clamp01(valence / 10f);
                float s = Mathf.Clamp01(arousal / 10f);
                Color suggested = Color.HSVToRGB(h, s, 1f);

                baseColorPicker.CurrentColor = suggested;
                baseColorPicker.onValueChanged.Invoke(suggested);
                glowColorPicker.CurrentColor = suggested;
                glowColorPicker.onValueChanged.Invoke(suggested);
            }
        }
    }
}
