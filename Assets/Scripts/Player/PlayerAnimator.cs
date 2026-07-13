using UnityEngine;

namespace Permafrost.Player
{
    /// <summary>
    /// todo. just everything
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Joint References")]
        [Tooltip("More specifically, the transform for moving the player's camera. This name is kinda misleading.")]
        [SerializeField] private Transform neckJoint;

        //private float baseNeckRotation;

        //[Header("Animations")]

        [Header("Component References")]
        [SerializeField] private Animator viewModelAnimator;

        private PlayerController playerController;
        private Rigidbody rb;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody>();
            //baseNeckRotation = neckJoint.localRotation.eulerAngles.y;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // neck rotation
            neckJoint.localRotation = Quaternion.Euler(-playerController.YLookAngle, -90, 0);
            //neckJoint.localRotation = Quaternion.AngleAxis(-playerController.YLookAngle /*+ baseNeckRotation*/, Vector3.right);

            if (debugEnabled)
            {
                Debug.Log($"[PlayerAnimator] Velocity: {rb.linearVelocity}, magnitude: {rb.linearVelocity.magnitude}");
            }
            viewModelAnimator.SetFloat("speed", rb.linearVelocity.magnitude);
        }
    }
}
