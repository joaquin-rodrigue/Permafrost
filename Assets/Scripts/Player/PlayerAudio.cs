using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Permafrost.Player
{
    [Serializable]
    public struct AudioSet
    {
        public float volume;
        public AudioClip[] sounds;
    }

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

        [Header("Component References")]
        [SerializeField] private AudioMixer audioMixer;
        //[SerializeField] private GameMaster gameMaster;
        //[SerializeField] private GroundCheck groundCheck;

        private PlayerController playerController;
        private Rigidbody rb;

        [Header("Miscellaneous")]
        [SerializeField] private float pausedMuffleThreshold;
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
            //if (gameMaster.GamePaused) { audioMixer.SetFloat("PauseMuffle",); return; }
            FootprintUpdate();
        }
        #endregion

        #region Audio Updating
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

        private void TemperatureAudioUpdate()
        {

        }
        #endregion

        #region Audio Calls

        #endregion
    }
}
