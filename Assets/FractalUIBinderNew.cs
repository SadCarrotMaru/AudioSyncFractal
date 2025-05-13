using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI.Extensions.ColorPicker;
public class FractalUIBinderNew : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown         sceneDropdown;
    public ColorPickerControl   baseColorPicker;
    public ColorPickerControl   glowColorPicker;

    [Header("Fractal Logic")]
    public FractalAnimation     fractalAnim;

    void Start()
    {
        var names = System.Enum.GetNames(typeof(FractalScene));
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(new List<string>(names));
        sceneDropdown.value = (int)fractalAnim.scene - 1;
        sceneDropdown.RefreshShownValue();
        sceneDropdown.onValueChanged.AddListener(OnSceneChanged);

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
    }

    private void OnSceneChanged(int idx)
    {
        // dropdown index 0→scene 1
        fractalAnim.scene = (FractalScene)(idx + 1);
        fractalAnim.targetRaymarcher.SetShaderScene((float)fractalAnim.scene);
    }
}