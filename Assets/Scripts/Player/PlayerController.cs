using Permafrost.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Permafrost.Player
{
    /// <summary>
    /// Handles all player controls, leaving them open for other components to
    /// effectively read the player's inputs. Also handles player physics 
    /// updating.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        #region Data
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float crouchSpeed = 3f;
        [SerializeField] private float sprintSpeed = 12f;
        [SerializeField] private float jumpForce = 8f;
        [Tooltip("How fast to accelerate the player downward per frame when jumping without holding the jump input.")]
        [SerializeField] private float fastFallAcceleration = 1f;
        [Tooltip("An acceleration factor for moving the player; the higher it is the faster the player reaches their target speed.")]
        [SerializeField] private float moveAcceleration = 1.5f;
        [Tooltip("TODO: to be moved to the PlayerAnimator. If I ever get done with that.")]
        [SerializeField] private float crouchAnimationSpeed = 1f;
        [Tooltip("Threshold for held inputs to be considered held. Really only affects gamepad trigger inputs.")]
        [SerializeField] private float inputPressedThreshold = 0.5f;

        [Header("Look")]
        [Tooltip("How sensitive to make the change in looking direction.")]
        [SerializeField] private float lookSensitivity = 2;
        [Tooltip("Smoothing factor for camera turning. 1 is no smoothing, anything more will progressively slow the camera movement.")]
        [Range(1f, 10f)]
        [SerializeField] private float lookSmoothing = 1.5f;

        private float currentSensitivity;
        private bool crouchRoutineActive = false;
        private int crouchRoutinesWaiting = 0;

        [Header("Component References")]
        [SerializeField] private DayNightCycle dayNightCycle;
        //[SerializeField] private GameMaster gameMaster;
        [SerializeField] private GroundChecker groundChecker;
        //[SerializeField] private PlayerInventory inventory;
        [SerializeField] private Transform cameraTransform;
        //[SerializeField] private UIController uiController;
        //[SerializeField] private PlayerHeldItemController viewModelController;

        private CapsuleCollider collision;
        private Rigidbody rb;
        private PlayerInput input;

        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference crouchAction;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference useItemAction;
        [SerializeField] private InputActionReference buildItemAction;
        [SerializeField] private InputActionReference dropItemAction;
        [SerializeField] private InputActionReference nextItemAction;
        [SerializeField] private InputActionReference previousItemAction;
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private InputActionReference pauseAction;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;

        // Input variables
        public Vector2 MoveInput { get; private set; }
        public bool SprintInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool Crouching { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool AttackHeld { get; private set; }
        public bool UsingItem { get; private set; }
        public bool BuildItem { get; private set; }
        public bool DropItem { get; private set; }
        public bool NextItem { get; private set; }
        public bool PreviousItem { get; private set; }
        public bool Interacting { get; private set; }
        public bool PauseInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        private int jumpHeldFrames;
        private int attackHeldFrames;
        private Vector2 currentLookVelocity;
        private Vector2 frameLookVelocity;
        #endregion

        #region Unity Methods
        // Setup
        private void Awake()
        {
            collision = GetComponent<CapsuleCollider>();
            input = GetComponent<PlayerInput>();
            rb = GetComponent<Rigidbody>();

            currentSensitivity = lookSensitivity;
            Cursor.lockState = CursorLockMode.Locked;

            // action!
            moveAction.action.performed += OnMoveInputEnter;
            moveAction.action.canceled += OnMoveInputExit;
            lookAction.action.performed += OnLookInputEnter;
            lookAction.action.canceled += OnLookInputExit;
            sprintAction.action.performed += OnSprintInputEnter;
            sprintAction.action.canceled += OnSprintInputExit;
            jumpAction.action.performed += OnJumpInputEnter;
            jumpAction.action.canceled += OnJumpInputExit;
            crouchAction.action.performed += OnCrouchInputEnter;
            crouchAction.action.canceled += OnCrouchInputExit;
            attackAction.action.performed += OnAttackInputEnter;
            attackAction.action.canceled += OnAttackInputExit;
            useItemAction.action.performed += OnUseItemInputEnter;
            useItemAction.action.canceled += OnUseItemInputExit;
            dropItemAction.action.performed += OnDropItemInputEnter;
            dropItemAction.action.canceled += OnDropItemInputExit;
            nextItemAction.action.performed += OnNextItemInputEnter;
            nextItemAction.action.canceled += OnNextItemInputExit;
            previousItemAction.action.performed += OnPreviousItemInputEnter;
            previousItemAction.action.canceled += OnPreviousItemInputExit;
            interactAction.action.performed += OnInteractInputEnter;
            interactAction.action.canceled += OnInteractInputExit;
            pauseAction.action.performed += OnPauseInputEnter;
            pauseAction.action.canceled += OnPauseInputExit;
            buildItemAction.action.performed += OnBuildItemInputEnter;
            buildItemAction.action.canceled += OnBuildItemInputExit;
        }

        // The normal update is only here to make sure currentSensitivity swaps when input
        // schemes are changed
        private void Update()
        {
            currentSensitivity = input.currentControlScheme.Equals("Keyboard&Mouse") ? lookSensitivity / 10 : lookSensitivity;
        }

        // Just make sure the player controls are active when this object is active
        // or inactive when it is not.
        private void OnEnable()
        {
            inputActions.FindActionMap("Player").Enable();
        }

        private void OnDisable()
        {
            inputActions.FindActionMap("Player").Disable();
        }
        #endregion

        #region Physics Update
        // Runs the physics update.
        private void FixedUpdate()
        {
            //if (gameMaster.GamePaused) return;
            if (PauseInput)
            {
                // ---------- TODO: pausing ----------
                Debug.LogWarning("pause input pressed, pause not implemented");
                return;
            }

            MoveUpdate();
            LookUpdate();
            JumpUpdate();
        }

        /// <summary>
        /// Updates the player's movement.
        /// </summary>
        private void MoveUpdate()
        {
            float targetSpeed = Crouching ? crouchSpeed : (SprintInput ? sprintSpeed : walkSpeed);
            Vector3 targetVelocity = targetSpeed * new Vector3(MoveInput.x, 0, MoveInput.y);
            float factor = Mathf.Clamp01(moveAcceleration / targetSpeed);
            Vector3 currentVelocity = rb.linearVelocity;

            // i love lerp who needs to create "acceleration" when you can just...
            // have a lerp do the job for you; its fast at lower speeds but slows down
            // and properly approaches your target when close to target speed.
            // it even basically clamps because as long as targetVelocity doesnt change
            // it'll just move towards it every time. beautiful and simple. why did I 
            // spend 2 hours trying to reinvent this wheel
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                transform.rotation * new Vector3(
                    targetVelocity.x,
                    rb.linearVelocity.y,
                    targetVelocity.z),
                factor);

            // todo: ground snapping using the groundChecker data
            
            if (debugEnabled)
            {
                Debug.Log($"[PlayerController] target vel: {targetVelocity}, current vel: {currentVelocity}, new vel: {rb.linearVelocity}");
            }
        }

        /// <summary>
        /// Updates the player camera direction.
        /// </summary>
        private void LookUpdate()
        {
            // ---------- TODO: uncomment the actual input when ready ----------
            if (/*!gameMaster.Paused && !uiCoontroller.AnyMenuActive*/ true)
            {
                Vector2 rawFrameVelocity = Vector2.Scale(LookInput, Vector2.one * currentSensitivity);
                frameLookVelocity = Vector2.Lerp(frameLookVelocity, rawFrameVelocity, 1 / lookSmoothing);
                currentLookVelocity += frameLookVelocity;
                currentLookVelocity.y = Mathf.Clamp(currentLookVelocity.y, -90, 90);
            }
            cameraTransform.localRotation = Quaternion.AngleAxis(-currentLookVelocity.y, Vector3.right);
            transform.localRotation = Quaternion.AngleAxis(currentLookVelocity.x, Vector3.up);
        }

        /// <summary>
        /// Handles player jumps and/or fastfall logic.
        /// </summary>
        private void JumpUpdate()
        {
            float currentVel = rb.linearVelocity.y;
            if (!JumpHeld && rb.linearVelocity.y > 0)
            {
                Vector3 fastFall = new(0, -fastFallAcceleration, 0);
                rb.AddRelativeForce(fastFall, ForceMode.Impulse);
            }
            
            if (JumpPressed && groundChecker.CanJump)
            {
                Vector3 jumpVelocity = new(0, jumpForce, 0);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddRelativeForce(jumpVelocity, ForceMode.Impulse);
            }
            
            if (JumpHeld)
            {
                jumpHeldFrames++;
                JumpPressed = jumpHeldFrames == 0;
            }

            if (debugEnabled)
            {
                Debug.Log($"[PlayerController] current vel: {currentVel}, fast fall: {(!JumpHeld && currentVel > 0)}, jumping: {(JumpPressed /*&& groundCheck.CanJump*/)}, new vel: {rb.linearVelocity.y}");
            }
        }

        /// <summary>
        /// Enters crouch mode.
        /// This is entirely placeholder until the player animator is done.
        /// </summary>
        private IEnumerator Crouch()
        {
            // just a way to buffer an uncrouch/crouch without
            // letting more than one buffer up in a row and create an
            // endless crouch/uncrouch loop from spamming
            if (crouchRoutinesWaiting > 1) yield break;
            crouchRoutinesWaiting++;
            while (crouchRoutineActive) yield return new WaitForFixedUpdate();
            crouchRoutinesWaiting--;
            Debug.Log("todo: temp crouch routine without animation");

            // animation!
            crouchRoutineActive = true;
            Vector3 center = Vector3.zero;
            Vector3 cam = cameraTransform.localPosition;

            for (float i = 0; i < 1; i += Time.fixedDeltaTime * crouchAnimationSpeed)
            {
                collision.height = 2 - i;
                center.y = -i / 2;
                collision.center = center;
                cam.y = -i * (3f / 4f) + 0.5f;
                cameraTransform.localPosition = cam;
                yield return new WaitForFixedUpdate();
            }

            // this part because float timers suck and we aren't 
            // guaranteed to hit the min
            // Not a siginificant issue, this code is going to go away
            // once the player animator is done and exists but oh well
            collision.height = 1;
            center.y = -0.5f;
            collision.center = center;
            cam.y = -0.25f;
            cameraTransform.localPosition = cam;
            crouchRoutineActive = false;
        }

        /// <summary>
        /// Exits crouch mode.
        /// </summary>
        private IEnumerator UnCrouch()
        {
            // just a way to buffer an uncrouch/crouch without
            // letting more than one buffer up in a row and create an
            // endless crouch/uncrouch loop from spamming
            if (crouchRoutinesWaiting > 1) yield break;
            crouchRoutinesWaiting++;
            while (crouchRoutineActive) yield return new WaitForFixedUpdate();
            crouchRoutinesWaiting--;
            Debug.Log("todo: temp crouch routine without animation");

            // animate
            crouchRoutineActive = true;
            Vector3 center = new(0, -0.5f, 0);
            Vector3 cam = cameraTransform.localPosition;

            for (float i = 0; i < 1; i += Time.fixedDeltaTime * crouchAnimationSpeed)
            {
                collision.height = 1 + i;
                center.y = i / 2 - 0.5f;
                collision.center = center;
                cam.y = i * (3f / 4f) - 0.25f;
                cameraTransform.localPosition = cam;
                yield return new WaitForFixedUpdate();
            }

            // fix it
            collision.height = 2;
            center.y = 0;
            collision.center = center;
            cam.y = 0.5f;
            cameraTransform.localPosition = cam;
            crouchRoutineActive = false;
        }
        #endregion

        #region Input Handlers
        private void OnMoveInputEnter(InputAction.CallbackContext ctx) => MoveInput = ctx.ReadValue<Vector2>();
        private void OnMoveInputExit(InputAction.CallbackContext ctx) => MoveInput = Vector2.zero;
        private void OnLookInputEnter(InputAction.CallbackContext ctx) => LookInput = ctx.ReadValue<Vector2>();
        private void OnLookInputExit(InputAction.CallbackContext ctx) => LookInput = Vector2.zero;
        private void OnSprintInputEnter(InputAction.CallbackContext ctx) => SprintInput = ctx.ReadValue<float>() > inputPressedThreshold;
        private void OnSprintInputExit(InputAction.CallbackContext ctx) => SprintInput = false;
        private void OnJumpInputEnter(InputAction.CallbackContext ctx)
        {
            JumpHeld = ctx.ReadValue<float>() > inputPressedThreshold;
            JumpPressed = jumpHeldFrames == 0;
        }
        private void OnJumpInputExit(InputAction.CallbackContext ctx)
        {
            JumpHeld = false;
            JumpPressed = false;
            jumpHeldFrames = 0;
        }
        private void OnCrouchInputEnter(InputAction.CallbackContext ctx)
        {
            Crouching = ctx.ReadValue<float>() > inputPressedThreshold;
            if (Crouching) StartCoroutine(Crouch());
            else StartCoroutine(UnCrouch());
        }
        private void OnCrouchInputExit(InputAction.CallbackContext ctx)
        {
            Crouching = false;
            StartCoroutine(UnCrouch());
        }
        private void OnAttackInputEnter(InputAction.CallbackContext ctx)
        {
            AttackHeld = ctx.ReadValue<float>() > inputPressedThreshold;
            AttackPressed = attackHeldFrames == 0;
        }
        private void OnAttackInputExit(InputAction.CallbackContext ctx)
        {
            AttackHeld = false;
            AttackPressed = false;
            attackHeldFrames = 0;
        }
        private void OnUseItemInputEnter(InputAction.CallbackContext ctx) => UsingItem = ctx.ReadValue<float>() > inputPressedThreshold;
        private void OnUseItemInputExit(InputAction.CallbackContext ctx) => UsingItem = false;
        private void OnDropItemInputEnter(InputAction.CallbackContext ctx) => DropItem = ctx.ReadValue<float>() > inputPressedThreshold;
        private void OnDropItemInputExit(InputAction.CallbackContext ctx) => DropItem = false;
        private void OnNextItemInputEnter(InputAction.CallbackContext ctx) => NextItem = ctx.ReadValue<float>() > inputPressedThreshold;
        private void OnNextItemInputExit(InputAction.CallbackContext ctx) => NextItem = false;
        private void OnPreviousItemInputEnter(InputAction.CallbackContext ctx) => PreviousItem = ctx.ReadValue<float>() > inputPressedThreshold;
        private void OnPreviousItemInputExit(InputAction.CallbackContext ctx) => PreviousItem = false;
        private void OnInteractInputEnter(InputAction.CallbackContext ctx) => Interacting = ctx.ReadValue<float>() > inputPressedThreshold;
        private void OnInteractInputExit(InputAction.CallbackContext ctx) => Interacting = false;
        private void OnPauseInputEnter(InputAction.CallbackContext ctx) => PauseInput = ctx.ReadValue<float>() > inputPressedThreshold;
        private void OnPauseInputExit(InputAction.CallbackContext ctx) => PauseInput = false;
        private void OnBuildItemInputEnter(InputAction.CallbackContext ctx) => BuildItem = ctx.ReadValue<float>() > inputPressedThreshold;
        private void OnBuildItemInputExit(InputAction.CallbackContext ctx) => BuildItem = false;
        #endregion
    }
}
