using Permafrost.Player;
using Permafrost.Items;

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Permafrost.UI
{
    /// <summary>
    /// Handler for all UI directly updated by the player components.
    /// </summary>
    public class PlayerUI : MonoBehaviour
    {
        #region Data
        [Header("Health")]
        [SerializeField] private Image healthBarShaded;
        [SerializeField] private Image healthBarBlack;
        [SerializeField] private GameObject healthBar;
        [SerializeField] private Material healthVignette;

        private Shader healthVignetteShader;

        [Header("Hunger")]
        [SerializeField] private Image hungerBarShaded;
        [SerializeField] private Image hungerBarBlack;
        [SerializeField] private GameObject hungerBar;

        [Header("Inventory")]
        [SerializeField] private GameObject inventoryBaseObject;
        [SerializeField] private TMP_Text[] inventorySlotTexts;
        [SerializeField] private Image[] inventorySlotSprites;

        [Header("Button Prompts")]
        [SerializeField] private GameObject[] buttonPromptObjects;
        [SerializeField] private RectTransform buttonPromptOrigin;
        [SerializeField] private Sprite[] buttonPromptSprites;
        [SerializeField] private string[] keyBindingsForPrompts;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;
        #endregion

        #region Unity Methods
        // Setup
        void Awake()
        {
            healthVignetteShader = healthVignette.shader;
        }
        #endregion

        #region Update the Player stuff
        /// <summary>
        /// Updates the player's health bar.
        /// </summary>
        /// <param name="health">The player's current health.</param>
        /// <param name="maxHealth">The player's current max health.</param>
        /// <param name="baseHealth">The player's base max health (usually 100).</param>
        public void UpdateHealthUI(float health, float maxHealth, float baseHealth)
        {
            if (health < 0) health = 0;
            healthBarBlack.rectTransform.localScale = new Vector3(maxHealth / baseHealth, 1, 1);
            healthBarShaded.rectTransform.localScale = new Vector3(health / baseHealth, 1, 1);

            healthVignette.SetFloat("_Power", (health / maxHealth) * 10);
        }

        /// <summary>
        /// Updates the player's hunger bar.
        /// </summary>
        /// <param name="hunger">The player's current hunger.</param>
        /// <param name="maxHunger">The player's current max hunger.</param>
        /// <param name="baseHunger">The player's base max hunger (usually 100).</param>
        public void UpdateHungerUI(float hunger, float maxHunger, float baseHunger)
        {
            if (hunger < 0) hunger = 0;
            hungerBarBlack.rectTransform.localScale = new Vector3(maxHunger / baseHunger, 1, 1);
            hungerBarShaded.rectTransform.localScale = new Vector3(hunger / baseHunger, 1, 1);
        }

        /// <summary>
        /// Updates the player inventory bar on the screen.
        /// </summary>
        /// <param name="inventory">The PlayerInventory that is being rendered.</param>
        /// <param name="selectedItem">The currently selected item.</param>
        public void UpdateInventoryUI(PlayerInventory inventory, int selectedItem)
        {
            for (int i = 0; i < inventorySlotSprites.Length; i++)
            {
                Item current = inventory.GetItem(i);

                // item exists
                if (current != null && current.GetCount() > 0)
                {
                    inventorySlotTexts[i].text = current.Stats.Name
                        + (current.GetCount() > 1 ? " x" + current.GetCount() : "");
                    inventorySlotSprites[i].sprite = current.Stats.InventoryRender;
                    inventorySlotSprites[i].gameObject.SetActive(true);
                }
                // item doesnt exist
                else
                {
                    inventorySlotTexts[i].text = "None";
                    inventorySlotSprites[i].gameObject.SetActive(false);
                }
                inventorySlotTexts[i].fontSize = 18;
            }
            inventorySlotTexts[selectedItem].fontSize = 27;
        }

        /// <summary>
        /// Updates the on-screen button prompts for the player.
        /// </summary>
        /// <param name="current">The player's currently held item.</param>
        /// <param name="playerPos">The player's transform, used to perform interact checks.</param>
        /// <param name="controls">The active PlayerController, used to get input bindings</param>
        public void UpdateButtonPrompts(Item current, Transform playerPos, Player.PlayerController controls)
        {
            foreach (GameObject prompt in buttonPromptObjects)
            {
                prompt.SetActive(false);
            }
            if (current == null || current.GetCount() == 0) return;

            // step 1: determine what prompts to do
            List<string> promptsNeeded = new();

            if (current.IsType(ItemType.Weapon)) promptsNeeded.Add("Attack");
            if (current.IsType(ItemType.Burnable)) promptsNeeded.Add("Create Fire");
            if (current.IsType(ItemType.Food)) promptsNeeded.Add("Eat");
            if (current.IsType(ItemType.Heal)) promptsNeeded.Add("Heal");

            // step 1.5: interact check
            RaycastHit hit;
            if (Physics.Raycast(playerPos.position, playerPos.forward, out hit, 5))
            {
                // TODO: interactable interface? idk if I can make an interface for this so
                // its probably gonna be a subclass of MonoBehaviour; still need to figure that out
                // if (hit.collider.TryGetComponent<Interactable>(out _)) promptsNeeded.Add("Interact");
            }

            // step 2: straight prompting it
            foreach (GameObject prompt in buttonPromptObjects)
            {
                if (promptsNeeded.Count <= 0) break;

                prompt.SetActive(true);
                prompt.GetComponentInChildren<TMP_Text>().text = promptsNeeded[0];
                InputBinding bind = controls.GetInputBinding(promptsNeeded[0]);
                if (debugEnabled)
                {
                    Debug.Log($"[PlayerUI]: binding for '{promptsNeeded[0]}': {bind.name}");
                }
            }
        }
        #endregion
    }
}
// 56 SLOC