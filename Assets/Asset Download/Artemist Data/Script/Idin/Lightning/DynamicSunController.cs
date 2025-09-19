// using UnityEngine;
// using UnityEngine.Rendering.Universal;

// public class SunCycle2D : MonoBehaviour
// {
//     public Light2D sunlight;
//     public float interval = 60f;
//     public float transitionDuration = 3f;

//     private float time;
//     private bool isDay = true;
//     private bool isTransitioning = false;
//     private float transitionTime = 0f;

//     // Target values for transition
//     private float startIntensity, targetIntensity;
//     private Color startColor, targetColor;
//     private Quaternion startRotation, targetRotation;

//     void Start()
//     {
//         if (PlayerPrefs.HasKey("SunCycleTime"))
//         {
//             time = PlayerPrefs.GetFloat("SunCycleTime");
//             isDay = PlayerPrefs.GetInt("SunCycleIsDay", 1) == 1;
//         }
//         else
//         {
//             isDay = true;
//             time = 0f;
//         }
//         ApplySunStateInstant();
//     }

//     void Update()
//     {
//         if (!isTransitioning)
//         {
//             time += Time.deltaTime;
//             if (time >= interval)
//             {
//                 time = 0f;
//                 isDay = !isDay;
//                 StartTransition();
//             }
//         }
//         else
//         {
//             transitionTime += Time.deltaTime;
//             float t = Mathf.Clamp01(transitionTime / transitionDuration);

//             sunlight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
//             sunlight.color = Color.Lerp(startColor, targetColor, t);
//             sunlight.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);

//             if (t >= 1f)
//             {
//                 isTransitioning = false;
//                 ApplySunStateInstant();
//             }
//         }
//     }

//     private void StartTransition()
//     {
//         isTransitioning = true;
//         transitionTime = 0f;

//         startIntensity = sunlight.intensity;
//         startColor = sunlight.color;
//         startRotation = sunlight.transform.rotation;

//         if (isDay)
//         {
//             targetIntensity = 1f;
//             targetColor = Color.white;
//             targetRotation = Quaternion.Euler(0, 0, 45);
//         }
//         else
//         {
//             targetIntensity = 0.2f;
//             targetColor = new Color(0.3f, 0.4f, 0.8f);
//             targetRotation = Quaternion.Euler(0, 0, -45);
//         }
//     }

//     private void ApplySunStateInstant()
//     {
//         if (isDay)
//         {
//             sunlight.intensity = 1f;
//             sunlight.color = Color.white;
//             sunlight.transform.rotation = Quaternion.Euler(0, 0, 45);
//         }
//         else
//         {
//             sunlight.intensity = 0.2f;
//             sunlight.color = new Color(0.3f, 0.4f, 0.8f);
//             sunlight.transform.rotation = Quaternion.Euler(0, 0, -45);
//         }
//     }

//     public void SaveSunCycle()
//     {
//         PlayerPrefs.SetFloat("SunCycleTime", time);
//         PlayerPrefs.SetInt("SunCycleIsDay", isDay ? 1 : 0);
//         PlayerPrefs.Save();
//     }

//     void OnDisable()
//     {
//         SaveSunCycle();
//     }
// }

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SunCycle2D : MonoBehaviour
{
    public Light2D sunlight;
    public Transform spotlightGroup;
    public float interval = 60f;
    public float transitionDuration = 3f;

    private float time;
    private bool isDay = true;
    private bool isTransitioning = false;
    private float transitionTime = 0f;

    // Target values for transition
    private float startIntensity, targetIntensity;
    private Color startColor, targetColor;
    private Quaternion startRotation, targetRotation;

    void Start()
    {
        if (PlayerPrefs.HasKey("SunCycleTime"))
        {
            time = PlayerPrefs.GetFloat("SunCycleTime");
            isDay = PlayerPrefs.GetInt("SunCycleIsDay", 1) == 1;
        }
        else
        {
            isDay = true;
            time = 0f;
        }
        ApplySunStateInstant();
        UpdateSpotlight();
    }

    void Update()
    {
        if (!isTransitioning)
        {
            time += Time.deltaTime;
            if (time >= interval)
            {
                time = 0f;
                isDay = !isDay;
                StartTransition();
            }
        }
        else
        {
            transitionTime += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTime / transitionDuration);

            sunlight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            sunlight.color = Color.Lerp(startColor, targetColor, t);
            sunlight.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);

            if (t >= 1f)
            {
                isTransitioning = false;
                ApplySunStateInstant();
                UpdateSpotlight();
            }
        }
    }

    private void StartTransition()
    {
        isTransitioning = true;
        transitionTime = 0f;

        startIntensity = sunlight.intensity;
        startColor = sunlight.color;
        startRotation = sunlight.transform.rotation;

        if (isDay)
        {
            targetIntensity = 1f;
            targetColor = Color.white;
            targetRotation = Quaternion.Euler(0, 0, 45);
        }
        else
        {
            targetIntensity = 0.2f;
            targetColor = new Color(0.3f, 0.4f, 0.8f);
            targetRotation = Quaternion.Euler(0, 0, -45);
        }
    }

    private void ApplySunStateInstant()
    {
        if (isDay)
        {
            sunlight.intensity = 1f;
            sunlight.color = Color.white;
            sunlight.transform.rotation = Quaternion.Euler(0, 0, 45);
        }
        else
        {
            sunlight.intensity = 0.2f;
            sunlight.color = new Color(0.3f, 0.4f, 0.8f);
            sunlight.transform.rotation = Quaternion.Euler(0, 0, -45);
        }
    }

    private void UpdateSpotlight()
    {
        if (spotlightGroup != null)
        {
            var spotlights = spotlightGroup.GetComponentsInChildren<Light2D>(true);
            foreach (var light in spotlights)
            {
                light.enabled = !isDay; // ON saat malam, OFF saat siang
            }
        }
    }

    public void SaveSunCycle()
    {
        PlayerPrefs.SetFloat("SunCycleTime", time);
        PlayerPrefs.SetInt("SunCycleIsDay", isDay ? 1 : 0);
        PlayerPrefs.Save();
    }

    void OnDisable()
    {
        SaveSunCycle();
    }
}