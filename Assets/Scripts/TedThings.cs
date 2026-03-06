using UnityEngine;

public class TedThings : MonoBehaviour
{
    [SerializeField] private AudioSource sound;
    [SerializeField] private float distanceMult;
    private GameObject player;
    private float playbackTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        playbackTimer += Time.deltaTime;
        if (playbackTimer > Vector3.Distance(player.transform.position, transform.position) * distanceMult + 1)
        {
            Debug.Log("playing");
            sound.Play();
            playbackTimer = 0;
        }
    }
}
