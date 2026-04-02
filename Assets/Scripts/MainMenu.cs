using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A simple script, just the actions the menu buttons have.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private Transform mainMenuPosition;
    [SerializeField] private Transform extrasMenuPosition;
    [SerializeField] private Transform controlsMenuPosition;
    [SerializeField] private GameObject cameraObj;
    [SerializeField] private GameObject mainMenuObj;
    [SerializeField] private GameObject extrasMenuObj;
    [SerializeField] private GameObject controlsMenuObj;
    [SerializeField] private int transitionLength;

    #region Extras Menu Transitions
    public void EnterExtrasMenu()
    {
        StartCoroutine(ExtrasMenuTransition());
    }

    private IEnumerator ExtrasMenuTransition()
    {
        mainMenuObj.SetActive(false);
        Vector3 totalPos = extrasMenuPosition.position - mainMenuPosition.position;
        Debug.Log(totalPos);
        Vector3 totalRot =  extrasMenuPosition.rotation.eulerAngles - mainMenuPosition.rotation.eulerAngles;
        Debug.Log(totalRot);
        
        for (int i = 0; i < transitionLength; i++)
        {
            cameraObj.transform.position += totalPos / transitionLength;
            cameraObj.transform.rotation = Quaternion.Euler(totalRot / transitionLength + cameraObj.transform.rotation.eulerAngles);
            yield return new WaitForFixedUpdate();
        }
        extrasMenuObj.SetActive(true);
    }

    public void ExitExtrasMenu()
    {
        StartCoroutine(ExtrasToMainTransition());
    }

    private IEnumerator ExtrasToMainTransition()
    {
        extrasMenuObj.SetActive(false);
        Vector3 totalPos = mainMenuPosition.position - extrasMenuPosition.position;
        Vector3 totalRot = mainMenuPosition.rotation.eulerAngles - extrasMenuPosition.rotation.eulerAngles;

        for (int i = 0; i < transitionLength; i++)
        {
            cameraObj.transform.position += totalPos / transitionLength;
            cameraObj.transform.rotation = Quaternion.Euler(totalRot / transitionLength + cameraObj.transform.rotation.eulerAngles);
            yield return new WaitForFixedUpdate();
        }
        mainMenuObj.SetActive(true);
    }
    #endregion

    #region Controls Menu Transitions
    public void EnterControlsMenu()
    {
        StartCoroutine(ControlsMenuTransition());
    }

    private IEnumerator ControlsMenuTransition()
    {
        mainMenuObj.SetActive(false);
        Vector3 totalPos = controlsMenuPosition.position - mainMenuPosition.position;
        Debug.Log(totalPos);
        Vector3 totalRot = controlsMenuPosition.rotation.eulerAngles - mainMenuPosition.rotation.eulerAngles;
        Debug.Log(totalRot);

        for (int i = 0; i < transitionLength; i++)
        {
            cameraObj.transform.position += totalPos / transitionLength;
            cameraObj.transform.rotation = Quaternion.Euler(totalRot / transitionLength + cameraObj.transform.rotation.eulerAngles);
            yield return new WaitForFixedUpdate();
        }
        controlsMenuObj.SetActive(true);
    }

    public void ExitControlsMenu()
    {
        StartCoroutine(ControlsToMainTransition());
    }

    private IEnumerator ControlsToMainTransition()
    {
        controlsMenuObj.SetActive(false);
        Vector3 totalPos = mainMenuPosition.position - controlsMenuPosition.position;
        Vector3 totalRot = mainMenuPosition.rotation.eulerAngles - controlsMenuPosition.rotation.eulerAngles;

        for (int i = 0; i < transitionLength; i++)
        {
            cameraObj.transform.position += totalPos / transitionLength;
            cameraObj.transform.rotation = Quaternion.Euler(totalRot / transitionLength + cameraObj.transform.rotation.eulerAngles);
            yield return new WaitForFixedUpdate();
        }
        mainMenuObj.SetActive(true);
    }
    #endregion

    public void QuitGame()
    {
        Application.Quit();
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
