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
        if (inside != null) inside.TransitionTo(transitionTime);
    }

    void OnTriggerExit(Collider other)
    {
        if (outside != null) outside.TransitionTo(transitionTime);
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
