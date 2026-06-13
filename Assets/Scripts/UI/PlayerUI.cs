using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Permafrost.UI
{
    public class PlayerUI : MonoBehaviour
    {
        #region Data
        [Header("Health")]
        [SerializeField] private Image healthBarShaded;
        [SerializeField] private GameObject healthBar;
        [SerializeField] private Material healthVignette;

        private Shader healthVignetteShader;

        [Header("Hunger")]
        [SerializeField] private Image hungerBarShaded;
        [SerializeField] private GameObject hungerBar;

        [Header("Inventory")]
        [SerializeField] private GameObject inventoryBaseObject;
        [SerializeField] private TMP_Text inventorySlotTexts;
        [SerializeField] private Image[] inventorySlotSprites;

        [Header("Button Prompts")]
        [SerializeField] private GameObject[] buttonPromptObjects;
        [SerializeField] private RectTransform buttonPromptOrigin;
        [SerializeField] private Sprite[] buttonPromptSprites;
        [SerializeField] private string[] keyBindingsForPrompts;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;
        #endregion

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
