using UnityEngine;

namespace Permafrost.Player
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This code was designed with Blender rigs in mind; Blender by default
    /// has different rotations for most things. Joints are generally rotated
    /// 90 degrees differently to what you would expect; z isn't the forward
    /// axis, its the right axis.
    /// </remarks>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Joint References")]
        [SerializeField] private Transform neckJoint;

        private float baseNeckRotation;

        //[Header("Animations")]

        [Header("Component References")]
        [SerializeField] private PlayerController playerController;

        private void Awake()
        {
            baseNeckRotation = neckJoint.localRotation.eulerAngles.z;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // neck rotation
            neckJoint.localRotation = Quaternion.AngleAxis(-playerController.YLookAngle + baseNeckRotation, Vector3.forward);
        }
    }
}
