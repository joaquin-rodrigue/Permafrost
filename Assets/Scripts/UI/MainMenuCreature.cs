using UnityEngine;

public class MainMenuCreature : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform endPosition;

    private Rigidbody rb;
    private Vector3 lookDir;
    [SerializeField] private Animator anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = startPosition.position;
        transform.LookAt(new Vector3(endPosition.position.x, transform.position.y, endPosition.position.z));
        lookDir = transform.rotation.eulerAngles;
        rb = GetComponent<Rigidbody>(); 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddRelativeForce(speed * Vector3.forward, ForceMode.Impulse);
        rb.linearVelocity = new Vector3(
            Mathf.Clamp(rb.linearVelocity.x, -speed, speed),
            rb.linearVelocity.y,
            Mathf.Clamp(rb.linearVelocity.z, -speed, speed)
        );
        anim.SetFloat("speed", speed);
        rb.angularVelocity = Vector3.zero;
        transform.eulerAngles = lookDir;

        if (Vector3.Distance(transform.position, endPosition.position) < 40f)
        {
            transform.position = startPosition.position;
        }
    }
}
