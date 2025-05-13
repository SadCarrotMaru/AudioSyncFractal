using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class FractalRuntimeUI : MonoBehaviour
{
    public FractalAnimation fractalAnim;
    
    void OnEnable()
    {
        // var uiDoc = GetComponent<UIDocument>();
        // var root  = uiDoc.rootVisualElement;
        // root.Clear();
        //
        // var bg = new VisualElement();
        // bg.style.position        = Position.Absolute;
        // bg.style.left            = 10;
        // bg.style.top             = 10;
        // bg.style.width           = 400;
        // bg.style.paddingLeft     = 10;
        // bg.style.paddingTop      = 10;
        // bg.style.backgroundColor = new StyleColor(new Color(0,0,0,0.5f));
        // bg.style.flexDirection   = FlexDirection.Column;
        // bg.style.alignItems      = Align.FlexStart;
        // bg.style.paddingBottom   = 4;
        // root.Add(bg);
        //
        // const int smallFz = 12;  // your desired font size
        //
        // // 1) Fractal Scene
        // var sceneField = new EnumField("Fractal Scene", fractalAnim.scene);
        // sceneField.Init(fractalAnim.scene);
        // sceneField.style.fontSize = smallFz;
        // sceneField.Q<Label>().style.color = new StyleColor(Color.white);
        // sceneField.Q<Label>().style.fontSize = smallFz;
        // sceneField.RegisterValueChangedCallback(evt => {
        //     fractalAnim.scene = (FractalScene)evt.newValue;
        //     fractalAnim.targetRaymarcher.SetShaderScene((float)fractalAnim.scene);
        // });
        // bg.Add(sceneField);
        //
        // // 2) Use Audio
        // var audioToggle = new Toggle("Use Audio") { value = fractalAnim.useAudio };
        // audioToggle.style.fontSize = smallFz;
        // audioToggle.Q<Label>().style.color = new StyleColor(Color.white);
        // audioToggle.RegisterValueChangedCallback(evt => fractalAnim.useAudio = evt.newValue);
        // bg.Add(audioToggle);
        //
        // // 3) AudioType
        // var audioTypeField = new EnumField("Audio Type", fractalAnim.audioType);
        // audioTypeField.Init(fractalAnim.audioType);
        // audioTypeField.style.fontSize = smallFz;
        // audioTypeField.Q<Label>().style.fontSize = smallFz;
        // audioTypeField.Q<Label>().style.color = new StyleColor(Color.white);
        // audioTypeField.RegisterValueChangedCallback(evt => fractalAnim.audioType = (AudioType)evt.newValue);
        // bg.Add(audioTypeField);
        //
        // // 4) Speed Slider
        // var speedField = new Slider("Speed", 0f, 5f) { value = fractalAnim.speed };
        // speedField.style.fontSize = smallFz;
        // speedField.Q<Label>().style.color = new StyleColor(Color.white);
        // speedField.RegisterValueChangedCallback(evt => fractalAnim.speed = evt.newValue);
        // bg.Add(speedField);
        //
        // // 5) Base Color
        // var colorField = new ColorField("Base Color") { value = fractalAnim.color };
        // colorField.style.fontSize = smallFz;
        // colorField.Q<Label>().style.color = new StyleColor(Color.white);
        // colorField.RegisterValueChangedCallback(evt => {
        //     fractalAnim.color = evt.newValue;
        //     fractalAnim.targetRaymarcher.SetShaderColor(evt.newValue);
        // });
        // bg.Add(colorField);
        //
        // // 6) Glow Color
        // var glowField = new ColorField("Glow Color") { value = fractalAnim.glow_color };
        // glowField.style.fontSize = smallFz;
        // glowField.Q<Label>().style.color = new StyleColor(Color.white);
        // glowField.RegisterValueChangedCallback(evt => {
        //     fractalAnim.glow_color = evt.newValue;
        //     fractalAnim.targetRaymarcher.SetShaderGlow(evt.newValue);
        // });
        // bg.Add(glowField);
    }
}
