using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Permafrost.Player
{
    /// <summary>
    /// A simple struct to combine the volume and sound clips into one object. 
    /// Mostly to clean up the code in here.
    /// </summary>
    [Serializable]
    public struct AudioSet
    {
        public float volume;
        public AudioClip[] sounds;
    }

    /// <summary>
    /// Controls all player sourced sound effects. Most of which can be called
    /// upon by other classes to play any of the sound effects of that kind.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAudio : MonoBehaviour
    {
        #region Data
        [Header("Audio Sources")]
        [SerializeField] private AudioSource footstepAudio;
        [SerializeField] private AudioSource heldItemAudio;
        [SerializeField] private AudioSource onHitAudio;
        [SerializeField] private AudioSource pickupAudio;
        [SerializeField] private AudioSource temperatureAudio;

        [Header("Footsteps")]
        [SerializeField] private AudioSet snowFootstepSetup;
        [SerializeField] private float distanceBetweenFootprints;
        private float footprintTimer;

        [Header("Held Item Stuff")]
        [SerializeField] private AudioSet eatingSetup;
        [SerializeField] private AudioSet meleeSwingSetup;

        [Header("On Hit Stuff")]
        [SerializeField] private AudioSet woodHitSetup;
        [SerializeField] private AudioSet creatureHitSetup;

        [Header("Pickup Stuff")]
        [SerializeField] private AudioSet pickupSetup;

        [Header("Temperature Stuff")]
        [SerializeField] private AudioSet coldTemperatureSetup;
        [SerializeField] private float coldSoundThreshold;

        [Header("Component References")]
        [SerializeField] private AudioMixer audioMixer;
        //[SerializeField] private GameMaster gameMaster;
        //[SerializeField] private GroundCheck groundCheck;
        [SerializeField] private PlayerStatus playerStatus;

        private PlayerController playerController;
        private Rigidbody rb;

        [Header("Miscellaneous")]
        [SerializeField] private float pausedMuffleThreshold;
        [SerializeField] private float universalMuffleThreshold;
        #endregion

        #region Unity Methods
        // Setup
        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            // unfortunately i need this for ONE linear velocity check.
            // hoping i find a way to get the velocity check done without
            // needing to reference the rigidbody in an otherwise unnecessary place...
            rb = GetComponent<Rigidbody>(); 
        }

        // Updating
        private void FixedUpdate()
        {
            //if (gameMaster.GamePaused) { audioMixer.SetFloat("PauseMuffle", pauseMuffleThreshold); return; }

            audioMixer.SetFloat("PauseMuffle", universalMuffleThreshold);
            FootprintUpdate();
        }
        #endregion

        #region Audio Updating
        /// <summary>
        /// Runs the code to update the timer for footprint sounds. And plays footprint sounds obv
        /// </summary>
        private void FootprintUpdate()
        {
            //if (!groundCheck.Grounded) return;

            footprintTimer += rb.linearVelocity.magnitude * Time.fixedDeltaTime;
            if (footprintTimer < distanceBetweenFootprints) return;

            Debug.Log("todo: determine audio type?");
            AudioSet setup = snowFootstepSetup; // do something to swap this out with other setups
            
            footstepAudio.volume = setup.volume;
            footstepAudio.PlayOneShot(setup.sounds[UnityEngine.Random.Range(0, setup.sounds.Length)]);
            footprintTimer -= distanceBetweenFootprints;
        }

        /// <summary>
        /// Will, in the future, affect sounds related to being too cold or too hot.
        /// </summary>
        private void TemperatureAudioUpdate()
        {
            Debug.Log("Todo: temperature audios??");
        }
        #endregion

        #region Audio Calls
        /// <summary>
        /// A helper function that takes a given source and plays the given setup on it.
        /// </summary>
        /// <param name="source">The AudioSource to play audio from.</param>
        /// <param name="setup">The AudioSet to use the volume of and one of the sound clips from selected at random.</param>
        private void PlayAudio(AudioSource source, AudioSet setup)
        {
            source.volume = setup.volume;
            source.PlayOneShot(setup.sounds[UnityEngine.Random.Range(0, setup.sounds.Length)]);
        }

        /// <summary>
        /// Plays a hit sound for hitting a creature with a melee attack.
        /// </summary>
        public void OnMeleeHitSound()
        {
            PlayAudio(onHitAudio, creatureHitSetup);
        }

        /// <summary>
        /// Plays a hit sound for hitting a tree or other wood object.
        /// </summary>
        public void OnTreeHitSound()
        {
            PlayAudio(onHitAudio, woodHitSetup);
        }

        /// <summary>
        /// Plays a sound for swinging a melee weapon.
        /// </summary>
        public void OnMeleeSwingSound()
        {
            PlayAudio(heldItemAudio, meleeSwingSetup);
        }

        /// <summary>
        /// Plays an eating sound. For eating something.
        /// </summary>
        public void OnEatSound()
        {
            PlayAudio(heldItemAudio, eatingSetup);
        }

        /// <summary>
        /// Plays a sound for picking up an item.
        /// </summary>
        public void OnPickupSound()
        {
            PlayAudio(pickupAudio, pickupSetup);
        }
        #endregion
    }
}
