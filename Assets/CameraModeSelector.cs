using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public enum CameraMode
{
    Normal,
    FreeView,
    Spectator
}

public class CameraModeSelector : MonoBehaviour
{
    [Header("UI Dropdown")]
    public TMP_Dropdown cameraModeDropdown;

    [Header("Camera Rigs")]
    public GameObject normalCameraRig;
    public GameObject freeViewCamera;
    public GameObject spectatorMainCamera;
    public GameObject spectatorShapesCamera;

    void Awake()
    {
        // 1) Pull the names from the enum
        var names = Enum.GetNames(typeof(CameraMode));
        var options = new List<string>(names);

        // 2) Populate the dropdown
        cameraModeDropdown.ClearOptions();
        cameraModeDropdown.AddOptions(options);

        // 3) Hook callback
        cameraModeDropdown.onValueChanged.AddListener(OnModeChanged);
    }

    void Start()
    {
        // Initialize to whatever the first enum is (Normal)
        OnModeChanged(cameraModeDropdown.value);
    }

    void OnModeChanged(int idx)
    {
        // (CameraMode)idx will map 0→Normal, 1→FreeView, 2→Spectator
        normalCameraRig.SetActive(idx == (int)CameraMode.Normal);
        freeViewCamera.SetActive(idx == (int)CameraMode.FreeView);
        spectatorMainCamera.SetActive(idx == (int)CameraMode.Spectator);
        spectatorShapesCamera.SetActive(idx == (int)CameraMode.Spectator);
    }
}