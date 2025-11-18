using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Enum to help keep track of what menu is active. Better than just using bools lol
/// </summary>
enum ActiveMenu
{
    None, PauseMenu, FireplaceMenu
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

    [Header("Player UI")]
    [SerializeField] private Image healthImage;
    [SerializeField] private Image hungerImage;
    [SerializeField] private TMP_Text[] inventorySlots;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject[] buttonPrompts;
    [SerializeField] private RectTransform buttonPromptOrigin;
    private bool playerUIActive = true;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Fireplace UI")]
    [SerializeField] private GameObject fireplaceMenu;
    [SerializeField] private TMP_Text fireStatusText;

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
    public void UpdateHungerUI(float hunger)
    {
        hungerImage.rectTransform.localScale = new Vector3(hunger / 100, hunger / 100, 1);
    }

    // Just changes the health image
    public void UpdateHealthUI(float health)
    {
        healthImage.rectTransform.localScale = new Vector3(health / 100, health / 100, 1);
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
    public void UpdateButtonPrompts(Item current)
    {
        foreach (GameObject prompt in buttonPrompts)
        {
            prompt.SetActive(false);
        }

        if (current == null) return;

        int activePrompts = 0;
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
                    break;
                }
            }
        }
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
                    break;
                }
            }
        }
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
                    break;
                }
            }
        }
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
                    break;
                }
            }
        }

        // todo: move the buttom prompt object so the prompts are centered
        buttonPromptOrigin.anchoredPosition = new Vector3(200 - (50 * activePrompts), 125, 0);
    }
    #endregion

    #region Pause Menu
    // Activate the pause menu UI
    public void ActivatePauseMenu()
    {
        if (playerUIActive)
        {
            healthImage.gameObject.SetActive(false);
            hungerImage.gameObject.SetActive(false);
            inventoryUI.SetActive(false);
            buttonPromptOrigin.gameObject.SetActive(false);
        }
        fireplaceMenu.SetActive(false);
        playerUIActive = false;
        pauseMenu.SetActive(true);
        activeMenu = ActiveMenu.PauseMenu;
    }

    // Deactivate the pause menu UI
    public void DeactivatePauseMenu()
    {
        if (!playerUIActive)
        {
            healthImage.gameObject.SetActive(true);
            hungerImage.gameObject.SetActive(true);
            inventoryUI.SetActive(true);
            buttonPromptOrigin.gameObject.SetActive(true);
        }
        playerUIActive = true;
        pauseMenu.SetActive(false);
        activeMenu = ActiveMenu.None;
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
}
