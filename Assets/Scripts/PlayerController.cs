using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
///     The controller for the player. Includes code for handling most player controls,
///     and player health and hunger.
/// </summary>
/// <remarks>
///     This class makes use of GetComponentsInChildren - thus, re-arranging the order of
///     the child components will probably break this script in places!
/// </remarks>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // Movement variables
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 8f;

    [SerializeField] private float health = 100;
    private bool isInLight;

    // Input variables
    private Vector2 moveInput;
    private bool jumpInput;
    private bool jumpHeld;
    private bool crouching;
    private bool canJump;
    private bool attacking;
    private bool canAttack = true;
    private bool usingItem;
    private bool nextItemInput;
    private bool previousItemInput;

    // Component references
    private PlayerInput playerInput;
    private Rigidbody rb;
    private Collider collision;
    private Collider meleeHitbox;
    private DayNightCycle daylight;
    private Image healthImage;

    // Inventory
    private List<string> inventory;
    private int selectedItem = 0;
    private int inventorySize;

    // UI
    private TMP_Text[] inventoryItemTexts;

    // Getting references to necessary objects + initializing the inventory system
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        collision = GetComponent<Collider>();
        daylight = GameObject.Find("Directional Light").GetComponent<DayNightCycle>();
        healthImage = GameObject.Find("Health Image").GetComponent<Image>();

        // horrible way to grab the first object w/ a trigger hitbox attached as a child
        // im probably over estimating how long this code takes to run, but its only runs once so...
        meleeHitbox = GetComponentsInChildren<Collider>().Where(collider => collider.isTrigger).First();
        meleeHitbox.enabled = false;

        inventorySize = 5;
        inventory = new List<string>();
        GameObject[] itemSlots = GameObject.FindGameObjectsWithTag("Inventory Slot");
        //Debug.Log(itemSlots.Length);
        inventoryItemTexts = new TMP_Text[itemSlots.Length];
        for (int i = 0; i < itemSlots.Length; i++)
        {
            inventoryItemTexts[i] = itemSlots[i].GetComponentInChildren<TMP_Text>();
        }
    }

    // Input handling happens here for more input accuracy
    void Update()
    {
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        jumpInput = playerInput.actions["Jump"].WasPressedThisFrame() || jumpInput;
        jumpHeld = playerInput.actions["Jump"].IsInProgress();
        crouching = playerInput.actions["Crouch"].IsInProgress();
        attacking = playerInput.actions["Attack"].WasPressedThisFrame() || attacking;
        usingItem = playerInput.actions["UseItem"].WasPressedThisFrame() || usingItem;
        nextItemInput = playerInput.actions["Next"].WasPressedThisFrame() || nextItemInput;
        previousItemInput = playerInput.actions["Previous"].WasPressedThisFrame() || previousItemInput;
    }

    // Processing what the player is doing this frame happens here for smoother and more consistent gameplay
    private void FixedUpdate()
    {
        //Debug.Log(attacking + " " + canAttack);
        // Check if the player is dead
        if (health <= 0)
        {
            Debug.LogError("you are ded. not big surprise");
            return;
        }

        // Moving
        float targetSpeed = moveSpeed;
        Vector2 targetVelocity = moveInput * targetSpeed;
        rb.linearVelocity = transform.rotation * new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.y);

        // Jump 
        if (jumpInput && canJump)
        {
            Vector3 jump = new Vector3(0, jumpHeight, 0);
            rb.AddRelativeForce(jump, ForceMode.Impulse);
            canJump = false;
        }

        jumpInput = false;

        // Attack
        if (attacking && canAttack)
        {
            StartCoroutine(MeleeAttack());
        }

        attacking = false;

        // The darkness consumes you
        if (daylight.LightValue < 0.25f && !isInLight)
        {
            Hurt(0.25f - daylight.LightValue);
        }

        // inventory management
        if (nextItemInput)
        {
            selectedItem++;
            if (selectedItem >= inventorySize)
            {
                selectedItem = 0;
            }
        }
        else if (previousItemInput)
        {
            selectedItem--;
            if (selectedItem < 0)
            {
                selectedItem = inventorySize - 1;
            }
        }

        nextItemInput = false;
        previousItemInput = false;
        // todo: drop

        // update the UI
        for (int i = 0; i < inventoryItemTexts.Length; i++)
        {
            if (inventory.Count > i)
            {
                inventoryItemTexts[i].text = inventory[i] ?? "None";
            }
            inventoryItemTexts[i].fontSize = 18;
        }
        inventoryItemTexts[selectedItem].fontSize = 27;
    }

    // The melee attack coroutine
    private IEnumerator MeleeAttack()
    {
        canAttack = false;
        meleeHitbox.enabled = true;
        yield return new WaitForSeconds(0.25f);
        meleeHitbox.enabled = false;
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }

    // Hurts the player
    public void Hurt(float damage)
    {
        health -= damage;
        Debug.Log(health);
        healthImage.rectTransform.localScale = new Vector3(health / 100, health / 100, 1);
    }

    // Adds an item to the inventory
    private void PickUpItem(GameObject item)
    {
        if (inventory.Count == inventorySize) 
        {
            return;
        }

        string theThingWeWant = item.GetComponent<Pickupable>().ItemName;
        inventory.Add(theThingWeWant);
        Debug.Log(theThingWeWant);
        Destroy(item);
    }

    // Uses the currently selected item
    private void UseItem()
    {
        if (inventory.Count <= selectedItem)
        {
            return;
        }

        // todo: really big switch block probably; gonna make this a bit more logical in the future
    }

    // Pretty simple ground check, probably not a good ground check so I'll fix that later
    private void OnCollisionEnter(Collision collision)
    {
        canJump = true;
    }

    // Detects whether the player is in a light source or not
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Light Source"))
        {
            isInLight = true;
        }

        if (other.GetComponent<Pickupable>() != null)
        {
            PickUpItem(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Light Source"))
        {
            isInLight = false;
        }
    }
}
