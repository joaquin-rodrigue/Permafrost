using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private float dayLength = 360;
    [SerializeField] private float time = 120;
    [SerializeField] private Light globalLight;
    [SerializeField] private LightingSettings settings;
    [SerializeField] private Color fogColorDay;
    // fogColorNight is just gonna be pitch black, so 0,0,0

    public float LightValue { get; private set; }

    // Update is called once per frame
    void Update()
    {
        float rotationAmount = 360 / dayLength * Time.deltaTime;
        time += Time.deltaTime;
        time %= dayLength;
        transform.Rotate(rotationAmount, 0, 0);

        // 0 = midnight; dayLength / 2 = noon
        LightValue = Mathf.Clamp(Mathf.Sin(Mathf.PI * time / dayLength), 0, 1);
        //Debug.Log(time + " " + LightValue);

        RenderSettings.ambientIntensity = LightValue;
        RenderSettings.fogColor = new Color(fogColorDay.r * LightValue, fogColorDay.g * LightValue, fogColorDay.b * LightValue);
        globalLight.intensity = LightValue + 0.01f;
    }
}
