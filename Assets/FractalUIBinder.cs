using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        speedSlider.onValueChanged.AddListener(v => fractalAnim.speed = v);

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
                useAudio = true; // set static variable
                songLabel.text = "Song: " + Path.GetFileNameWithoutExtension(fullPath);
            }
        }
    }
}
