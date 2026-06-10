using UnityEngine;

namespace Permafrost.Player
{
    /// <summary>
    /// 
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Joint References")]
        [Tooltip("More specifically, the transform for moving the player's camera. This name is kinda misleading.")]
        [SerializeField] private Transform neckJoint;

        //private float baseNeckRotation;

        //[Header("Animations")]

        [Header("Component References")]
        [SerializeField] private PlayerController playerController;

        private void Awake()
        {
            //baseNeckRotation = neckJoint.localRotation.eulerAngles.y;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // neck rotation
            neckJoint.localRotation = Quaternion.Euler(-playerController.YLookAngle, -90, 0);
            //neckJoint.localRotation = Quaternion.AngleAxis(-playerController.YLookAngle /*+ baseNeckRotation*/, Vector3.right);
        }
    }
}
