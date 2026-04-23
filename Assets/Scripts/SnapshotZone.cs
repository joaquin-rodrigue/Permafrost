using UnityEngine;
using UnityEngine.Audio;

public class SnapshotZone : MonoBehaviour
{
    public AudioMixerSnapshot inside;
    public AudioMixerSnapshot outside;
    public AudioMixerSnapshot start;
    public float transitionTime = 1f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (inside != null)
        {
            inside.TransitionTo(transitionTime);
            //Debug.Log("transition to " + inside + " " + gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (outside != null)
        {
            outside.TransitionTo(transitionTime);
            //Debug.Log("transition to " + outside);
        }
    }

    public void StartMenuSound()
    {
        start.TransitionTo(0);
    }

    public void StartMenuExit()
    {
        outside.TransitionTo(1f);
    }
}
