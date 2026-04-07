using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private float fadeInOutTime = 2f;
    [SerializeField] private Image blackground;
    private bool fadingOut;
    private Scene loadCaller;

    private void Awake()
    {
        loadCaller = SceneManager.GetSceneAt(0);
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private void FixedUpdate()
    {
        if (fadingOut) return;
        if (!SceneManager.GetSceneAt(0).Equals(loadCaller)) StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float co = 1f / fadeInOutTime;
        Color col = new(0, 0, 0, 0);
        for (float i = 0; i < fadeInOutTime; i += Time.fixedDeltaTime)
        {
            col.a += co;
            blackground.color = col;
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator FadeOut()
    {
        float co = 1f / fadeInOutTime;
        Color col = new(0, 0, 0, 1);
        for (float i = 0; i < fadeInOutTime; i += Time.fixedDeltaTime)
        {
            col.a -= co;
            blackground.color = col;
            yield return new WaitForFixedUpdate();
        }
        SceneManager.UnloadSceneAsync("Loading");
    }
}
