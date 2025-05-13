using UnityEngine;
using TMPro;

public class FractalUIBinder : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown        sceneDropdown;
    public FlexibleColorPicker baseColorPicker;
    public FlexibleColorPicker glowColorPicker;

    [Header("Fractal Script")]
    public FractalAnimation    fractalAnim;

    void Start()
    {
        // Populate the dropdown
        var names = System.Enum.GetNames(typeof(FractalScene));
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(new System.Collections.Generic.List<string>(names));
        sceneDropdown.value = (int)fractalAnim.scene - 1;
        sceneDropdown.RefreshShownValue();
        sceneDropdown.onValueChanged.AddListener(OnSceneChanged);

        // Initialize the pickers to the fractal’s current colors
        if (baseColorPicker != null)
        {
            baseColorPicker.color = fractalAnim.color;
            baseColorPicker.onColorChange.AddListener(OnBaseColorChanged);
        }

        if (glowColorPicker != null)
        {
            glowColorPicker.color = fractalAnim.glow_color;
            glowColorPicker.onColorChange.AddListener(OnGlowColorChanged);
        }
    }

    private void OnSceneChanged(int idx)
    {
        fractalAnim.scene = (FractalScene)(idx + 1);
        fractalAnim.targetRaymarcher.SetShaderScene((float)fractalAnim.scene);
    }

    private void OnBaseColorChanged(Color c)
    {
        fractalAnim.color = c;
        fractalAnim.targetRaymarcher.SetShaderColor(c);
    }

    private void OnGlowColorChanged(Color c)
    {
        fractalAnim.glow_color = c;
        fractalAnim.targetRaymarcher.SetShaderGlow(c);
    }
}