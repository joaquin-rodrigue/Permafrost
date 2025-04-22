using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private float dayLength = 60;
    [SerializeField] private Light globalLight;
    [SerializeField] private LightingSettings settings;
    [SerializeField] private Color fogColorDay;
    // fogColorNight is just gonna be pitch black, so 0,0,0

    public float LightValue { get; private set; }

    // Update is called once per frame
    void Update()
    {
        float rotationAmount = 360 / dayLength * Time.deltaTime;
        transform.Rotate(0, rotationAmount, 0);

        LightValue = Mathf.Clamp((1 - Mathf.Abs(transform.rotation.y)) * 2 - 0.5f, 0, 1);
        RenderSettings.ambientIntensity = LightValue;
        RenderSettings.fogColor = new Color(fogColorDay.r * LightValue, fogColorDay.g * LightValue, fogColorDay.b * LightValue);
    }
}
