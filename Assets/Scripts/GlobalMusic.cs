using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GlobalMusic : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private float smoothTime;
    [SerializeField] private float leadVolume;
    [SerializeField] private float bassVolume;
    [SerializeField] private float fastVolume;

    private GameObject player;
    private float currentLeadVolume = -60;
    private float currentBassVolume = -60;
    private float currentFastVolume = -60;

    private float targetLeadVolume;
    private float targetBassVolume;
    private float targetFastVolume;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnNewSceneLoad;

        targetLeadVolume = leadVolume;
        targetBassVolume = bassVolume;
        targetFastVolume = fastVolume;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnNewSceneLoad(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.Find("Player");
        if (player == null) player = GameObject.Find("Main Camera");

        targetLeadVolume = leadVolume;
        targetBassVolume = bassVolume;
        targetFastVolume = fastVolume;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;
        transform.position = player.transform.position;

        mixer.SetFloat("LeadVolume", Mathf.Lerp(currentLeadVolume, targetLeadVolume, 1 / smoothTime));
        mixer.GetFloat("LeadVolume", out currentLeadVolume);
        mixer.SetFloat("BassVolume", Mathf.Lerp(currentBassVolume, targetBassVolume, 1 / smoothTime));
        mixer.GetFloat("BassVolume", out currentBassVolume);
        mixer.SetFloat("FastVolume", Mathf.Lerp(currentFastVolume, targetFastVolume, 1 / smoothTime));
        mixer.GetFloat("FastVolume", out currentFastVolume);

    }

    public void SetLeadVolume(float newLead)
    {
        targetLeadVolume = newLead;
    }

    public void SetBassVolume(float newBass)
    {
        targetBassVolume = newBass;
    }

    public void SetFastVolume(float newFast)
    {
        targetFastVolume = newFast;
    }
}
