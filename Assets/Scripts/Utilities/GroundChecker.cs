using UnityEngine;

namespace Permafrost.Utilities
{
    /// <summary>
    /// Universal ground checking component for any objects that need it.
    /// Used for players but also by most enemies.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    public class GroundChecker : MonoBehaviour
    {
        #region Data
        public static readonly float MAX_GROUND_CHECK_DISTANCE = 1000f;

        [Header("Settings")]
        [SerializeField] private LayerMask groundLayers;
        [Tooltip("The amount of distance between this object's position and the ground while flush with the ground.")]
        [SerializeField] private float restingDistanceFromGround;
        [SerializeField] private float minDistanceToBeGrounded;
        [SerializeField] private float coyoteTime;

        private float coyoteJumpTimer;
        public float DistanceToGround { get; private set; }
        public bool Grounded { get; private set; }
        public bool CanJump { get; private set; }

        [Header("Component References")]
        [SerializeField] private GameMaster gameMaster;
        [SerializeField] private Collider groundCollider;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;
        #endregion

        #region Unity Methods
        // Setup
        private void OnEnable()
        {
            Grounded = false;
            CanJump = false;
            DistanceToGround = 0;
        }

        // yup
        private void FixedUpdate()
        {
            if (gameMaster.GamePaused) return;

            CheckForGround();
        }
        #endregion

        #region Updating the ground check
        /// <summary>
        /// The function that checks whether the object is grounded.
        /// </summary>
        private void CheckForGround()
        {
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, MAX_GROUND_CHECK_DISTANCE, groundLayers, QueryTriggerInteraction.Ignore);
            DistanceToGround = hit.distance - restingDistanceFromGround;
            Grounded = DistanceToGround < minDistanceToBeGrounded;
            coyoteJumpTimer = Grounded ? 0 : coyoteJumpTimer + Time.fixedDeltaTime;
            CanJump = Grounded || coyoteJumpTimer < coyoteTime;

            if (debugEnabled)
            {
                Debug.Log($"[GroundCheck] distance: {DistanceToGround}, grounded: {Grounded}, can jump: {CanJump}");
            }
        }

        // To make sure that ground checking happens when the object actually collides with the ground.
        private void OnCollisionEnter(Collision collision)
        {
            int num = groundLayers.value & collision.gameObject.layer;
            if (num != 0)
            {
                // for the case we check ground one frame, aren't grounded,
                // but then the next frame we collide with the ground
                CheckForGround();
            }
        }
        #endregion
    }
}
// 22 SLOC