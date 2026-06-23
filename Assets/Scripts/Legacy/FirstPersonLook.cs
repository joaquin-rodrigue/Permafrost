using UnityEngine;
using UnityEngine.InputSystem;

[System.Obsolete]
public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] private Transform character;
    [SerializeField] private PlayerController controls;
    [SerializeField] private PlayerInput input;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;

    private void Start()
    {
        if (input.currentControlScheme == "Keyboard&Mouse")
        {
            sensitivity /= 10;
        }
    }

    void Update()
    {
        Vector2 lookInput = input.actions["Look"].ReadValue<Vector2>();
        if (controls.IsPaused || controls.InteractUIActive)
        {
            velocity += Vector2.zero;
        }
        else
        {
            // todo: if im reclaiming this code it needs to use the new input system
            // Get smooth velocity.
            Vector2 mouseDelta = lookInput; //new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
            frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
            velocity += frameVelocity;
            velocity.y = Mathf.Clamp(velocity.y, -90, 90);
        }

        // Rotate camera up-down and controller left-right from velocity.
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }
}
