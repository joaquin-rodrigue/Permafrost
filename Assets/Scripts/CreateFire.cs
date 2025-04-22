using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CreateFire : MonoBehaviour
{
    [SerializeField] private GameObject fireplace;
    private GameObject currentFire;
    private PlayerInput playerInput;
    private bool createFireButton;
    private bool canPlaceFire = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        createFireButton = playerInput.actions["Fire"].WasPressedThisFrame() || createFireButton;
    }

    private void FixedUpdate()
    {
        //Debug.Log(currentFire);
        //Debug.Log(createFireButton);
        if (createFireButton && canPlaceFire)
        {
            StartCoroutine(PlaceFireCooldown());
            if (currentFire == null)
            {
                currentFire = Instantiate(fireplace, transform.position + Vector3.forward * 2, Quaternion.identity);
            }
            else
            {
                currentFire.transform.position = transform.position + Vector3.forward * 2;
            }
        }

        createFireButton = false;
    }

    private IEnumerator PlaceFireCooldown()
    {
        canPlaceFire = false;
        yield return new WaitForSeconds(5f);
        canPlaceFire = true;
    }
}
