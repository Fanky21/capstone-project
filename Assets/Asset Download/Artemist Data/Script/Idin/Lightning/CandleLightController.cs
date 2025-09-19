using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CandleLightController : MonoBehaviour
{
    public Light2D light2D;

    [Header("Radius Settings")]
    public float outerMin = 2.3f;
    public float outerMax = 2.8f;
    public float innerMin = 0.4f;
    public float innerMax = 0.5f;

    [Header("Falloff Settings")]
    public float falloffMin = 0.4f;
    public float falloffMax = 0.5f;

    [Header("Intensity Settings")]
    public float intensityMin = 0.9f;
    public float intensityMax = 1.7f;

    [Header("Speed Settings")]
    public float pulseSpeed = 1.6f;

    private float time;

    void Update()
    {
        if (light2D == null) return;

        time += Time.deltaTime * pulseSpeed;

        float t = (Mathf.Sin(time) + 1f) / 2f;

        light2D.pointLightOuterRadius = Mathf.Lerp(outerMin, outerMax, t);
        light2D.pointLightInnerRadius = Mathf.Lerp(innerMin, innerMax, t);
        light2D.falloffIntensity = Mathf.Lerp(falloffMin, falloffMax, t);
        light2D.intensity = Mathf.Lerp(intensityMin, intensityMax, t); // Efek terang-redup
    }
}
