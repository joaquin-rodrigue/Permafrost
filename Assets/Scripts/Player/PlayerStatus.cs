using System.Collections;
using UnityEngine;

namespace Permafrost.Player
{
    /// <summary>
    /// Handles all storage of player stats, including health and hunger.
    /// For health related reasons, also keeps track of whether the player is
    /// in darkness and drains health while so.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerStatus : MonoBehaviour
    {
        #region Data
        [Header("Health")]
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private float invulnerabilityTime = 0.35f;
        [Tooltip("When the player is at full hunger, this much health will be healed per frame, decreasing based on the hunger regen threshold.")]
        [SerializeField] private float fullHungerRegen = 0.1f;

        public float MaxHealth { get => maxHealth; }
        public float CurrentHealth { get; private set; }
        public bool InvulnerabilityActive { get; private set; }
        public bool Dead { get; private set; }

        [Header("Hunger")]
        [SerializeField] private float maxHunger = 100;
        [Tooltip("How much damage to deal to the player per frame when their hunger is empty.")]
        [SerializeField] private float starveDamage = 0.05f;
        [Tooltip("The minimum amount of hunger the player needs to have regen. See fullHungerRegen.")]
        [SerializeField] private float hungerRegenThreshold = 90;
        [Tooltip("The amount of hunger drained compared to health regened.")]
        [SerializeField] private float regenHungerDrainRatio = 1.5f;
        [SerializeField] private float passiveHungerLoss = 0.003f;
        [SerializeField] private float sprintHungerLoss = 0.009f;

        public float MaxHunger { get => maxHunger; }
        public float CurrentHunger { get; private set; }
        private float maxToRegenThresholdDiff;

        [Header("Darkness Things")]
        [Tooltip("How dark it has to be for the player to start taking damage. 1 = full daylight, 0 = pitch black darkness.")]
        [Range(0f, 1f)]
        [SerializeField] private float darknessDamageThreshold = 0.25f;
        [Tooltip("Increases darkness damage dealt by this number times how many seconds the player has been in darkness, per frame.")]
        [SerializeField] private float darknessTimeMultiplier = 0.1f;

        public float DarknessTimer { get; private set; }
        public bool IsInLight { get; private set; }

        [Header("Temperature")]
        [SerializeField] private float TODO;

        [Header("Component References")]
        [SerializeField] private DayNightCycle dayNightCycle;
        //[SerializeField] private GameMaster gameMaster;
        [SerializeField] private UIController uiController;

        private PlayerController playerController;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;
        #endregion

        #region Unity Methods
        // Setup
        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            CurrentHealth = maxHealth;
            CurrentHunger = maxHunger;
            maxToRegenThresholdDiff = maxHunger - hungerRegenThreshold;
        }

        // Basically just enabled i frames when entering the scene.
        private void OnEnable()
        {
            StartCoroutine(InvulnerabilityToggle());
        }

        // Update the stuff
        private void FixedUpdate()
        {
            //if (gameMaster.GamePaused) return;

            HungerUpdate();
            DarknessUpdate();
        }

        // All 3 trigger functions are to update the IsInLight status.
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Light Source"))
            {
                IsInLight = true;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Light Source"))
            {
                IsInLight = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Light Source"))
            {
                IsInLight = false;
            }
        }
        #endregion

        #region Updating the Stats
        /// <summary>
        /// Drains the players hunger. If near full health, applies regen, and
        /// if at no hunger, applies starving damage.
        /// </summary>
        private void HungerUpdate()
        {
            CurrentHunger -= (playerController.SprintInput && !playerController.Crouching) ? sprintHungerLoss : passiveHungerLoss;

            if (CurrentHunger > hungerRegenThreshold && CurrentHealth < maxHealth)
            {
                // regen
                float percent = (CurrentHunger - hungerRegenThreshold) / maxToRegenThresholdDiff;
                float regen = fullHungerRegen * percent;
                CurrentHealth += regen;
                if (CurrentHealth > maxHealth) CurrentHealth = maxHealth;
                CurrentHunger -= regen * regenHungerDrainRatio;
            }
            else if (CurrentHunger < 0)
            {
                // starve
                CurrentHunger = 0;
                CurrentHealth -= starveDamage;
            }
        }

        /// <summary>
        /// If the player is in darkness, updates the darkness timer and damages player.
        /// </summary>
        private void DarknessUpdate()
        {
            // todo: determine if its dark - i think I need to expand this to a full function
            if (dayNightCycle.LightValue > darknessDamageThreshold || IsInLight) return;

            DarknessTimer += Time.fixedDeltaTime;
            float damage = darknessDamageThreshold - dayNightCycle.LightValue + DarknessTimer * darknessTimeMultiplier;
            CurrentHealth -= damage;
        }
        #endregion

        #region Hurt/Heal
        /// <summary>
        /// Hurts the player if i-frames aren't active.
        /// Activates i-frames if damage is applied.
        /// </summary>
        /// <param name="damage">How much damage to deal to the player.</param>
        /// <returns>True if the damage dealt kills the player, false otherwise.</returns>
        public bool Hurt(int damage)
        {
            if (InvulnerabilityActive) return false;

            CurrentHealth -= damage;
            StartCoroutine(InvulnerabilityToggle());

            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
                Dead = true;
            }
            return CurrentHealth == 0;
        }

        /// <summary>
        /// Hurts the player, regardless of i-frames.
        /// Will not activate i-frames.
        /// </summary>
        /// <param name="damage">How much damage to deal to the player.</param>
        /// <returns>True if the damage dealt kills the player, false otherwise.</returns>
        public bool HurtContinuous(int damage)
        {
            CurrentHealth -= damage;
            // no i frames this time
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
                Dead = true;
            }
            return CurrentHealth == 0;
        }

        /// <summary>
        /// Heals the player.
        /// </summary>
        /// <param name="health">How much health to heal.</param>
        public void Heal(int health)
        {
            CurrentHealth += health;
            if (CurrentHealth > maxHealth) CurrentHealth = maxHealth;
        }

        /// <summary>
        /// Restores the player's hunger.
        /// </summary>
        /// <param name="hunger">How much hunger to restore.</param>
        public void Eat(int hunger)
        {
            CurrentHunger += hunger;
            if (CurrentHunger > maxHunger) CurrentHunger = maxHunger;
        }

        /// <summary>
        /// Drains the player's hunger.
        /// </summary>
        /// <param name="hunger">How much hunger to drain.</param>
        public void Hunger(int hunger)
        {
            CurrentHunger -= hunger;
            if (CurrentHunger < maxHunger) CurrentHunger = 0;
        }

        /// <summary>
        /// Toggles i-frames on, then toggles them off after invulnerabilityTime seconds.
        /// </summary>
        /// <returns>After invulnerabilityTime seconds.</returns>
        private IEnumerator InvulnerabilityToggle()
        {
            InvulnerabilityActive = true;
            yield return new WaitForSeconds(invulnerabilityTime);
            InvulnerabilityActive = false;
        }
        #endregion
    }
}