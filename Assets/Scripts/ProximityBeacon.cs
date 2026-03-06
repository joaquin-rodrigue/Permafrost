using UnityEngine;

public class ProximityBeacon : MonoBehaviour
{
    public Transform player;
    public float rateScale = 5f;

    private AudioSource source;
    private float timer;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        float interval = distance / rateScale;

        //timer += Time.deltaTime;

        //Debug.Log(interval + " " + timer);
        if (!source.isPlaying)
        {
            source.pitch = 1 / Mathf.Max(1, Mathf.Log10(interval));
            //timer = 0f;
            source.Play();
        }
    }
}

