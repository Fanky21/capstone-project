using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    [Header("Lighting")]
    public Light2D sunlight;
    public Transform spotlightGroup;

    [Header("Time Settings")]
    [Tooltip("Berapa detik dunia nyata untuk 1 hari penuh (24 jam dunia).")]
    public float fullDayDuration = 120f; // 2 menit = 1 hari penuh

    [Tooltip("Durasi transisi saat fajar/senja (detik dunia nyata).")]
    public float transitionDuration = 3f;

    [Header("UI")]
    public TextMeshProUGUI jamDunia;

    private float worldTime; 
    private bool isDay = true;
    private bool isTransitioning = false;
    private float transitionTime;

    private float startIntensity, targetIntensity;
    private Color startColor, targetColor;

    void Start()
    {
        if (PlayerPrefs.HasKey("WorldTime"))
            worldTime = PlayerPrefs.GetFloat("WorldTime", 360f);

        UpdateLightingInstant();
        UpdateSpotlight();
        UpdateJamUI();
    }

    void Update()
    {
        // jalankan waktu dunia
        worldTime += Time.deltaTime * (1440f / fullDayDuration);

        if (worldTime >= 1440f) worldTime -= 1440f; // reset ke 0 (00:00)

        UpdateJamUI();

        // deteksi jam untuk transisi
        int hour = Mathf.FloorToInt(worldTime / 60f);

        if (!isTransitioning)
        {
            if (hour == 6 && !isDay)
            {
                isDay = true;
                StartTransition(true);  // sunrise
            }
            else if (hour == 18 && isDay)
            {
                isDay = false;
                StartTransition(false); // sunset
            }
        }

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
        if (spotlightGroup != null)
        {
            var lights = spotlightGroup.GetComponentsInChildren<Light2D>(true);
            foreach (var light in lights)
                light.enabled = !isDay;
        }
    }

    private void UpdateJamUI()
    {
        if (jamDunia == null) return;

        int hours = Mathf.FloorToInt(worldTime / 60f);
        int minutes = Mathf.FloorToInt(worldTime % 60f);
        jamDunia.text = $"{hours:00} : {minutes:00}";
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("WorldTime", worldTime);
        PlayerPrefs.Save();
    }
}
