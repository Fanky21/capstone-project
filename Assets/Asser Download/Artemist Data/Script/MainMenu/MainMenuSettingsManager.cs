using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class MainMenuSettingsManager : MonoBehaviour
{
    // Get User Settings Data

    // Graphics Device Settings
    string[] availableGraphics;
    int currentGraphicIndex = 0;

    // Monitor Settings
    int currentMonitorIndex = 0;
    Resolution[] availableResolutions;

    // Settings Title Labels
    public TMP_Text settingsVideo;
    public TMP_Text settingsAudio;
    public TMP_Text settingsGame;
    public TMP_Text settingsOther;

    // Video Settings
    public TMP_Text graphicLabel;
    public TMP_Text displayModeLabel;
    public TMP_Text monitorLabel;
    public TMP_Text vsyncLabel;
    public TMP_Text resolutionLabel;
    public TMP_Text graphicValue;
    public TMP_Text displayModeValue;
    public TMP_Text monitorValue;
    public TMP_Text vsyncValue;
    public TMP_Text resolutionValue;

    int currentResolutionIndex = 0;

    // GPU Detector class (unchanged)
    public class GPUDetector : MonoBehaviour
    {
        public string[] availableGraphics;

        void Start()
        {
            availableGraphics = GetGPUsWindows();
        }

        string[] GetGPUsWindows()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c wmic path win32_VideoController get name";
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Process process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string[] lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1) return new string[] { SystemInfo.graphicsDeviceName };

            string[] gpuNames = new string[lines.Length - 1];
            for (int i = 1; i < lines.Length; i++)
            {
                gpuNames[i - 1] = lines[i].Trim();
            }
            return gpuNames;
        }
    }

    void Start()
        {
            // Activate all connected displays so they work on build
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

        availableGraphics = new string[] { SystemInfo.graphicsDeviceName };
        currentGraphicIndex = 0;
        UpdateGraphicDeviceText();

        currentMonitorIndex = 0;
        availableResolutions = GetUniqueResolutions(Screen.resolutions);

        UpdateFullscreenText();
        UpdateMonitorText();
        UpdateVsyncText();
        UpdateResolutionText();

        // Add scroll + hover handlers to all value texts
        AddScrollChange(graphicValue, (next) => { if (next) NextGraphicDevice(); else PrevGraphicDevice(); });
        AddScrollChange(displayModeValue, (next) => { ToggleFullscreen(); });
        AddScrollChange(monitorValue, (next) => { if (next) NextMonitor(); else PrevMonitor(); });
        AddScrollChange(vsyncValue, (next) => { ToggleVSync(); });
        AddScrollChange(resolutionValue, (next) => { if (next) NextResolution(); else PrevResolution(); });
    }

    Resolution[] GetUniqueResolutions(Resolution[] resolutions)
    {
        var unique = new List<Resolution>();
        var seen = new HashSet<string>();

        foreach (var res in resolutions)
        {
            string key = res.width + "x" + res.height;
            if (!seen.Contains(key))
            {
                seen.Add(key);
                unique.Add(res);
            }
        }

        return unique.ToArray();
    }

    void AddScrollChange(TMP_Text text, Action<bool> callback)
    {
        var scrollComp = text.gameObject.GetComponent<ScrollChangeOnHover>();
        if (scrollComp == null)
            scrollComp = text.gameObject.AddComponent<ScrollChangeOnHover>();

        scrollComp.targetText = text;
        scrollComp.OnScrollChange = callback;
        scrollComp.normalColor = Color.white;
        scrollComp.hoverColor = Color.cyan;
    }

    // GPU

    public void NextGraphicDevice()
    {
        currentGraphicIndex = (currentGraphicIndex + 1) % availableGraphics.Length;
        UpdateGraphicDeviceText();
    }

    public void PrevGraphicDevice()
    {
        currentGraphicIndex--;
        if (currentGraphicIndex < 0)
            currentGraphicIndex = availableGraphics.Length - 1;

        UpdateGraphicDeviceText();
    }

    void UpdateGraphicDeviceText()
    {
        graphicValue.text = "< " + availableGraphics[currentGraphicIndex] + " >";

        RectTransform rt = graphicValue.GetComponent<RectTransform>();
        rt.localScale = Vector3.one * 1.1f;
        LeanTween.scale(rt, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutBack);
    }

    // --- Fullscreen toggle ---
    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        UpdateFullscreenText();
    }

    void UpdateFullscreenText()
    {
        displayModeValue.text = Screen.fullScreen ? "< ON >" : "< OFF >";
        AnimateLabel(displayModeValue);
    }

    // --- Monitor selection ---
    public void NextMonitor()
    {
        currentMonitorIndex = (currentMonitorIndex + 1) % Display.displays.Length;
        UpdateMonitorText();
        ApplyMonitorAndResolution();
    }

    public void PrevMonitor()
    {
        currentMonitorIndex--;
        if (currentMonitorIndex < 0)
            currentMonitorIndex = Display.displays.Length - 1;

        UpdateMonitorText();
        ApplyMonitorAndResolution();
    }

    void UpdateMonitorText()
    {
        string monitorName = $"Screen {currentMonitorIndex + 1}";

        monitorValue.text = $"< {monitorName} >";
        AnimateLabel(monitorValue);
    }

    // --- VSync toggle ---
    public void ToggleVSync()
    {
        if (QualitySettings.vSyncCount == 0)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;

        UpdateVsyncText();
    }

    void UpdateVsyncText()
    {
        vsyncValue.text = QualitySettings.vSyncCount > 0 ? "< ON >" : "< OFF >";
        AnimateLabel(vsyncValue);
    }

    // --- Resolution selection ---

    public void NextResolution()
    {
        currentResolutionIndex = (currentResolutionIndex + 1) % availableResolutions.Length;
        UpdateResolutionText();
        ApplyResolution();
    }

    public void PrevResolution()
    {
        currentResolutionIndex--;
        if (currentResolutionIndex < 0)
            currentResolutionIndex = availableResolutions.Length - 1;

        UpdateResolutionText();
        ApplyResolution();
    }

    void UpdateResolutionText()
    {
        Resolution res = availableResolutions[currentResolutionIndex];
        resolutionValue.text = $"< {res.width} x {res.height} >";
        AnimateLabel(resolutionValue);
    }

    void ApplyMonitorAndResolution()
    {
        Resolution res = availableResolutions[currentResolutionIndex];
        bool isFullscreen = Screen.fullScreen;

        Screen.SetResolution(res.width, res.height, isFullscreen);
    }

    void ApplyResolution()
    {
        ApplyMonitorAndResolution();
    }

    // Animate label scaling helper
    void AnimateLabel(TMP_Text label)
    {
        RectTransform rt = label.GetComponent<RectTransform>();
        rt.localScale = Vector3.one * 1.1f;
        LeanTween.scale(rt, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutBack);
    }

    // void Update()
    // {
    //     // Keyboard input optional - you can remove if only scroll is desired
    //     if (Input.GetKeyDown(KeyCode.RightArrow))
    //     {
    //         NextGraphicDevice();
    //     }
    //     else if (Input.GetKeyDown(KeyCode.LeftArrow))
    //     {
    //         PrevGraphicDevice();
    //     }

    //     if (Input.GetKeyDown(KeyCode.F))
    //         ToggleFullscreen();

    //     if (Input.GetKeyDown(KeyCode.M))
    //         NextMonitor();

    //     if (Input.GetKeyDown(KeyCode.V))
    //         ToggleVSync();

    //     if (Input.GetKeyDown(KeyCode.RightArrow))
    //         NextResolution();

    //     if (Input.GetKeyDown(KeyCode.LeftArrow))
    //         PrevResolution();
    // }
}

// Separate component to handle scroll & hover on a UI text element
public class ScrollChangeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text targetText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.cyan;

    public Action<bool> OnScrollChange; // true = next, false = prev

    bool isHovering = false;

    void Update()
    {
        if (isHovering)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (scroll > 0)
            {
                OnScrollChange?.Invoke(true);
            }
            else if (scroll < 0)
            {
                OnScrollChange?.Invoke(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (targetText != null)
            targetText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (targetText != null)
            targetText.color = normalColor;
    }
}
