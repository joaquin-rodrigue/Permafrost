using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// todO: please fix
/// </summary>
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
        Debug.Log("fading");
        float co = 1f / fadeInOutTime;
        Color col = new(0, 0, 0, 0);
        for (float i = 0; i < fadeInOutTime; i += Time.fixedDeltaTime)
        {
            col.a += co;
            blackground.color = col;
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("fading done");
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("fading");
        float co = 1f / fadeInOutTime;
        Color col = new(0, 0, 0, 1);
        for (float i = 0; i < fadeInOutTime; i += Time.fixedDeltaTime)
        {
            col.a -= co;
            blackground.color = col;
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("fading done");
        SceneManager.UnloadSceneAsync("Loading");
    }
}
