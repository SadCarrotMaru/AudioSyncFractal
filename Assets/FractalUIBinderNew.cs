using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;               // ← for Slider
using TMPro;
using UnityEngine.UI.Extensions.ColorPicker;  // ← for ColorPickerControl

public class FractalUIBinderNew : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown         sceneDropdown;
    public Slider               speedSlider;
    public ColorPickerControl   baseColorPicker;
    public ColorPickerControl   glowColorPicker;

    [Header("Fractal Logic")]
    public FractalAnimation     fractalAnim;

    void Start()
    {
        // — Scene Dropdown —
        var names = System.Enum.GetNames(typeof(FractalScene));
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(new List<string>(names));
        sceneDropdown.value = (int)fractalAnim.scene - 1;
        sceneDropdown.RefreshShownValue();
        sceneDropdown.onValueChanged.AddListener(OnSceneChanged);

        speedSlider.minValue = 0f;
        speedSlider.maxValue = 5f;
        speedSlider.value    = fractalAnim.speed;
        speedSlider.onValueChanged.AddListener(v => {
            fractalAnim.speed = v;
        });

        // — Base Color Picker —
        baseColorPicker.CurrentColor = fractalAnim.color;
        baseColorPicker.onValueChanged.AddListener(c =>
        {
            fractalAnim.color = c;
            fractalAnim.targetRaymarcher.SetShaderColor(c);
        });
        
        // — Glow Color Picker —
        glowColorPicker.CurrentColor = fractalAnim.glow_color;
        glowColorPicker.onValueChanged.AddListener(c =>
        {
            fractalAnim.glow_color = c;
            fractalAnim.targetRaymarcher.SetShaderGlow(c);
        });
    }

    private void OnSceneChanged(int idx)
    {
        fractalAnim.scene = (FractalScene)(idx + 1);
        fractalAnim.targetRaymarcher.SetShaderScene((float)fractalAnim.scene);
    }
}