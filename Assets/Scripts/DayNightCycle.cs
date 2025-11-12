using UnityEngine;

/// <summary>
/// Main handler for day/night cycle behaviour.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float dayLength = 360;
    [SerializeField] private float time = 120;

    [Header("Object References")]
    [SerializeField] private Light globalLight;
    [SerializeField] private LightingSettings settings;
    [SerializeField] private Color fogColorDay;

    /// <summary>
    /// The current light value. This is a float between 0 and 1.
    /// </summary>
    public float LightValue { get; private set; }

    void Update()
    {
        time += Time.deltaTime;
        time %= dayLength;
        float rotation = (time / dayLength) * 360 - 90;
        transform.rotation = Quaternion.Euler(rotation, 0, 0);

        // 0 = midnight; dayLength / 2 = noon
        LightValue = Mathf.Clamp(Mathf.Sin(Mathf.PI * time / dayLength) * 2 - 0.75f, 0, 1);
        //Debug.Log(time + " " + LightValue + " " + transform.rotation.eulerAngles);

        RenderSettings.ambientIntensity = LightValue;
        RenderSettings.fogColor = new Color(fogColorDay.r * LightValue, fogColorDay.g * LightValue, fogColorDay.b * LightValue);
        globalLight.intensity = LightValue + 0.01f;
    }
}
