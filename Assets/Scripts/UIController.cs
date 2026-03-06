using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
///     Enum to help keep track of what menu is active. Better than just using bools lol
/// </summary>
enum ActiveMenu
{
    None, PauseMenu, FireplaceMenu, CarMenu, DeathScreen
}

/// <summary>
///     Class to combine all UI management in one file. Just keeps things more organized.
/// </summary>
/// <remarks>
///     Not that this was necessary, but this was an unintentional fix for UI slots being 
///     in the wrong spots. Turns out, programmatically assigning things is usually worse
///     than hard-coding the references through serialization. Usually.
/// </remarks>
public class UIController : MonoBehaviour
{
    private ActiveMenu activeMenu;

    // Player
    [Header("Player UI")]
    [SerializeField] private Image healthImage;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Image hungerImage;
    [SerializeField] private GameObject hungerBar;
    [SerializeField] private TMP_Text[] inventorySlots;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject[] buttonPrompts;
    [SerializeField] private RectTransform buttonPromptOrigin;
    private bool playerUIActive = true;

    // Keybinding images
    [SerializeField] private Sprite[] buttonsForPrompts;
    [SerializeField] private string[] keyBindingsForPrompts;

    // Menus
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private TMP_Text errorMessageText;

    [Header("Fireplace UI")]
    [SerializeField] private GameObject fireplaceMenu;
    [SerializeField] private TMP_Text fireStatusText;

    [Header("Car UI")]
    [SerializeField] private GameObject carMenu;
    [SerializeField] private TMP_Text fuelCansText;

    [Header("Death UI")]
    [SerializeField] private GameObject deathScreen;

    [Header("Debug items")]
    [SerializeField] private bool enableFpsMeter;
    [SerializeField] private TMP_Text fpsMeter;

    private void Start()
    {
        // FPS meter is basically a debug option
        if (fpsMeter != null && enableFpsMeter)
        {
            InvokeRepeating(nameof(FPSMeterUpdate), 0.1f, 0.1f);
        }
    }

    #region Player UI Updates
    // Just changes the hunger image
    public void UpdateHungerUI(float hunger, float maxHunger)
    {
        if (hunger < 0) hunger = 0;
        hungerImage.rectTransform.localScale = new Vector3(hunger / maxHunger, 1, 1);
    }

    // Just changes the health image
    public void UpdateHealthUI(float health, float maxHealth)
    {
        if (health < 0) health = 0;
        healthImage.rectTransform.localScale = new Vector3(health / maxHealth, 1, 1);
    }

    // Updates all the inventory sections
    public void UpdateInventoryUI(Item[] inventory, int selectedItem)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventory[i] != null)
            {
                inventorySlots[i].text = inventory[i].Stats.Name + (inventory[i].GetCount() > 1 ? " x" + inventory[i].GetCount() : "");
            }
            else
            {
                inventorySlots[i].text = "None";
            }
            inventorySlots[i].fontSize = 18;
        }
        inventorySlots[selectedItem].fontSize = 27;
    }

    // Updates the button prompts based on the player's currently held item
    // TODO: this code has some problems; the whole two list thing *would* be a map in a
    // good world but unfortunately this is Unity and dictionaries aren't serializable and
    // won't show up in the editor! yippee!! but anyways this code still needs work and
    // it sucks
    public void UpdateButtonPrompts(Item current, Transform playerPos)
    {
        foreach (GameObject prompt in buttonPrompts)
        {
            prompt.SetActive(false);
        }

        if (current == null) return;

        int activePrompts = 0;
        // --- weapon items ---
        if (current.IsType(ItemType.Weapon))
        {
            foreach (GameObject prompt in buttonPrompts)
            {
                if (!prompt.activeSelf)
                {
                    prompt.SetActive(true);
                    activePrompts++;
                    // todo: make the prompt an "Attack" prompt
                    prompt.GetComponentInChildren<TMP_Text>().text = "Attack";
                    int index = -1;
                    for (int i = 0; i < keyBindingsForPrompts.Length; i++)
                    {
                        if (keyBindingsForPrompts[i] == "LeftClick")
                        {
                            index = i; break;
                        }
                    }
                    prompt.GetComponentInChildren<Image>().sprite = buttonsForPrompts[index];
                    break;
                }
            }
        }
        // --- flammable items ---
        if (current.IsType(ItemType.Burnable))
        {
            foreach (GameObject prompt in buttonPrompts) 
            {
                if (!prompt.activeSelf)
                {
                    activePrompts++;
                    prompt.SetActive(true);
                    // todo: make the prompt a "Create Fire" prompt
                    prompt.GetComponentInChildren<TMP_Text>().text = "Create Fire";
                    int index = -1;
                    for (int i = 0; i < keyBindingsForPrompts.Length; i++)
                    {
                        if (keyBindingsForPrompts[i] == "Q")
                        {
                            index = i; break;
                        }
                    }
                    prompt.GetComponentInChildren<Image>().sprite = buttonsForPrompts[index];
                    break;
                }
            }
        }
        // --- food items --- 
        if (current.IsType(ItemType.Food))
        {
            foreach (GameObject prompt in buttonPrompts)
            {
                if (!prompt.activeSelf)
                {
                    activePrompts++;
                    prompt.SetActive(true);
                    // todo: make the prompt a "Eat" prompt
                    prompt.GetComponentInChildren<TMP_Text>().text = "Eat";
                    int index = -1;
                    for (int i = 0; i < keyBindingsForPrompts.Length; i++)
                    {
                        if (keyBindingsForPrompts[i] == "RightClick")
                        {
                            index = i; break;
                        }
                    }
                    prompt.GetComponentInChildren<Image>().sprite = buttonsForPrompts[index];
                    break;
                }
            }
        }
        // --- tree cutter items --- 
        if (current.IsType(ItemType.TreeChop))
        {
            foreach (GameObject prompt in buttonPrompts)
            {
                if (!prompt.activeSelf)
                {
                    activePrompts++;
                    prompt.SetActive(true);
                    // todo: make the prompt a "Chop Tree" prompt
                    prompt.GetComponentInChildren<TMP_Text>().text = "Chop Tree";
                    int index = -1;
                    for (int i = 0; i < keyBindingsForPrompts.Length; i++)
                    {
                        if (keyBindingsForPrompts[i] == "LeftClick")
                        {
                            index = i; break;
                        }
                    }
                    prompt.GetComponentInChildren<Image>().sprite = buttonsForPrompts[index];
                    break;
                }
            }
        }
        // --- healing items ---
        if (current.IsType(ItemType.Heal))
        {
            foreach (GameObject prompt in buttonPrompts)
            {
                if (!prompt.activeSelf)
                {
                    activePrompts++;
                    prompt.SetActive(true);
                    prompt.GetComponentInChildren<TMP_Text>().text = "Heal";
                    int index = -1;
                    for (int i = 0; i < keyBindingsForPrompts.Length; i++)
                    {
                        if (keyBindingsForPrompts[i] == "RightClick")
                        {
                            index = i; break;
                        }
                    }
                    prompt.GetComponentInChildren<Image>().sprite = buttonsForPrompts[index];
                    break;
                }
            }
        }

        RaycastHit hit;
        // --- interact --- 
        if (Physics.Raycast(playerPos.position, playerPos.forward, out hit, 5))
        {
            if (hit.collider.TryGetComponent<FireLifespan>(out _) || hit.collider.TryGetComponent<TheCar>(out _)) {
                foreach (GameObject prompt in buttonPrompts)
                {
                    if (!prompt.activeSelf)
                    {
                        activePrompts++;
                        prompt.SetActive(true);
                        prompt.GetComponentInChildren<TMP_Text>().text = "Interact";
                        int index = -1;
                        for (int i = 0; i < keyBindingsForPrompts.Length; i++)
                        {
                            if (keyBindingsForPrompts[i] == "E")
                            {
                                index = i; break;
                            }
                        }
                        prompt.GetComponentInChildren<Image>().sprite = buttonsForPrompts[index];
                        break;
                    }
                }
            }
        }

        // todo: move the buttom prompt object so the prompts are centered
        buttonPromptOrigin.anchoredPosition = new Vector3(375 - (75 * activePrompts * 1.25f), 160, 0);
    }
    #endregion

    #region Pause + Death Menu
    // Activate the pause menu UI
    public void ActivatePauseMenu()
    {
        if (playerUIActive)
        {
            healthBar.SetActive(false);
            hungerBar.SetActive(false);
            inventoryUI.SetActive(false);
            buttonPromptOrigin.gameObject.SetActive(false);
        }
        fireplaceMenu.SetActive(false);
        carMenu.SetActive(false);
        playerUIActive = false;
        pauseMenu.SetActive(true);
        activeMenu = ActiveMenu.PauseMenu;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // reset because death scene
        SceneManager.LoadScene(0);
    }

    // Deactivate the pause menu UI
    public void DeactivatePauseMenu()
    {
        if (!playerUIActive)
        {
            healthBar.SetActive(true);
            hungerBar.SetActive(true);
            inventoryUI.SetActive(true);
            buttonPromptOrigin.gameObject.SetActive(true);
        }
        playerUIActive = true;
        pauseMenu.SetActive(false);
        activeMenu = ActiveMenu.None;
    }

    // Activates the death screen
    public void ActivateDeathMenu()
    {
        activeMenu = ActiveMenu.DeathScreen;
        fireplaceMenu.SetActive(false);
        carMenu.SetActive(false);
        pauseMenu.SetActive(false);
        deathScreen.SetActive(true);
    }
    #endregion

    #region Fireplace UI
    public void OpenFireplaceUI(FireLifespan fireData)
    {
        if (activeMenu == ActiveMenu.PauseMenu || fireData == null)
        {
            return;
        }
        fireplaceMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        switch (fireData.GetFuel()) 
        {
            case > 180:
                fireStatusText.text = "This fire will last a while.";
                break;
            case > 120:
                fireStatusText.text = "This fire has some fuel left.";
                break;
            case > 80:
                fireStatusText.text = "This fire is running out of fuel.";
                break;
            case > 40:
                fireStatusText.text = "This fire is almost burnt out.";
                break;
        }
    }

    public void CloseFireplaceUI()
    {
        fireplaceMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion

    #region Car UI
    public void OpenCarUI(TheCar car)
    {
        if (activeMenu == ActiveMenu.CarMenu || car == null)
        {
            return;
        }
        carMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        int fuel = car.FuelCount();
        fuelCansText.text = fuel + "/10 cans added";
    }

    public void CloseCarUI()
    {
        carMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion

    #region Miscellaneous
    // Updates the FPS meter
    private void FPSMeterUpdate()
    {
        float fps = 1f / Time.deltaTime;
        fpsMeter.text = "FPS: " + fps;
        if (fps > 60) fpsMeter.color = Color.green;
        else if (fps > 40) fpsMeter.color = Color.yellow;
        else if (fps > 20) fpsMeter.color = new Color(1, 0.44f, 0.02f);
        else fpsMeter.color = Color.red;
    }

    public void SetErrorText(string text)
    {
        StopCoroutine(nameof(TurnOffErrorText));
        errorMessageText.text = text;
        errorMessageText.gameObject.SetActive(true);
        StartCoroutine(TurnOffErrorText());
    }

    private IEnumerator TurnOffErrorText()
    {
        yield return new WaitForSeconds(5f);
        errorMessageText.gameObject.SetActive(false);
    }
    #endregion
}
