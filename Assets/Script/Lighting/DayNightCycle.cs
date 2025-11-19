using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance;

    [Header("Time Settings")]
    public float fullDayDuration = 120f; // detik real untuk 24 jam dunia
    public float WorldTime { get; private set; } // menit 0–1440

    [Header("Lighting")]
    public Light2D sunlight;
    public Transform spotlightGroup;

    [Header("Transition")]
    public float transitionDuration = 3f;

    private bool isDay = true;
    private bool isTransitioning = false;
    private float transitionTime;
    private float startIntensity, targetIntensity;
    private Color startColor, targetColor;

    private void Awake()
    {
        Instance = this;

        // Load waktu dari PlayerPrefs
        WorldTime = PlayerPrefs.GetFloat("WorldTime", 360f);
    }
    void Start()
    {
        if (Instance == null) return;

        // Sync time
        float worldTime = WorldTime;
        int hour = Mathf.FloorToInt(worldTime / 60f);

        // Determine correct state
        if (hour >= 6 && hour < 18)
            isDay = true;
        else
            isDay = false;

        // Apply lighting
        UpdateLightingInstant();
        UpdateSpotlight();
    }

    private void Update()
    {
        // update waktu dunia
        WorldTime += Time.deltaTime * (1440f / fullDayDuration);
        if (WorldTime >= 1440f) WorldTime -= 1440f;

        int hour = Mathf.FloorToInt(WorldTime / 60f);

        // kontrol sunrise / sunset
        if (!isTransitioning)
        {
            if (hour == 6 && !isDay)
            {
                isDay = true;
                StartTransition(true);
            }
            else if (hour == 18 && isDay)
            {
                isDay = false;
                StartTransition(false);
            }
        }

        // transisi lighting
        if (isTransitioning)
        {
            transitionTime += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTime / transitionDuration);

            sunlight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            sunlight.color = Color.Lerp(startColor, targetColor, t);

            if (t >= 1f)
            {
                isTransitioning = false;
                UpdateLightingInstant();
                UpdateSpotlight();
            }
        }
    }

    private void StartTransition(bool toDay)
    {
        isTransitioning = true;
        transitionTime = 0f;

        startIntensity = sunlight.intensity;
        startColor = sunlight.color;

        if (toDay)
        {
            targetIntensity = 1f;
            targetColor = Color.white;
        }
        else
        {
            targetIntensity = 0.2f;
            targetColor = new Color(0.3f, 0.4f, 0.8f);
        }
    }

    private void UpdateLightingInstant()
    {
        if (isDay)
        {
            sunlight.intensity = 1f;
            sunlight.color = Color.white;
        }
        else
        {
            sunlight.intensity = 0.2f;
            sunlight.color = new Color(0.3f, 0.4f, 0.8f);
        }
    }

    private void UpdateSpotlight()
    {
        if (spotlightGroup == null) return;

        var lights = spotlightGroup.GetComponentsInChildren<Light2D>(true);
        foreach (var l in lights)
            l.enabled = !isDay;
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("WorldTime", WorldTime);
        PlayerPrefs.Save();
    }
}
