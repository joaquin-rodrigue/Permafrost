using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private float hunger = 100;
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
    private DayNightCycle daylight;
    private Image healthImage;
    private Image hungerImage;

    // Inventory
    private Item[] inventory;
    private int selectedItem = 0;
    private int inventorySize;

    // UI
    private TMP_Text[] inventoryItemTexts;

    // Weapon related
    [SerializeField] private GameObject meleeWeapon;
    private int CurrentDamage;

    // Fire spawning
    [SerializeField] private GameObject fireplace;
    [SerializeField] private GameObject fireSpawnCheck;
    private GameObject currentFire;
    private bool createFireButton;
    private bool canPlaceFire = true;

    #region Start + Update(s)
    // Getting references to necessary objects + initializing the inventory system
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        collision = GetComponent<Collider>();
        daylight = GameObject.Find("Directional Light").GetComponent<DayNightCycle>();
        healthImage = GameObject.Find("Health Image").GetComponent<Image>();
        hungerImage = GameObject.Find("Food Image").GetComponent<Image>();

        // The getcomponentsinchildren call here is gone, instead it must be set in the editor
        meleeWeapon.SetActive(false);

        inventorySize = 5;
        inventory = new Item[inventorySize];
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
        createFireButton = playerInput.actions["Fire"].WasPressedThisFrame() || createFireButton;
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

        // The hunger also consumes you
        hunger -= 0.01f;
        if (hunger <= 0)
        {
            hunger = 0;
            Hurt(0.05f);
        }
        hungerImage.rectTransform.localScale = new Vector3(hunger / 100, hunger / 100, 1);

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

        // Use item - all the code is done in UseItem
        if (usingItem)
        {
            UseItem();
        }

        // update the UI
        for (int i = 0; i < inventoryItemTexts.Length; i++)
        {
            if (inventory[i] != null)
            {
                inventoryItemTexts[i].text = inventory[i].GetAttributes().Name + (inventory[i].GetCount() > 1 ? " x" + inventory[i].GetCount() : "");
            }
            else
            {
                inventoryItemTexts[i].text = "None";
            }
            inventoryItemTexts[i].fontSize = 18;
        }
        inventoryItemTexts[selectedItem].fontSize = 27;

        // Create a fire
        if (createFireButton && canPlaceFire)
        {
            CreateFire();
        }

        createFireButton = false;
    }
    #endregion

    #region Weapon/Hurt Behavior
    // The melee attack coroutine
    // TODO: replace this with a call for melee attack animation on the given weapon equipped
    private IEnumerator MeleeAttack()
    {
        canAttack = false;
        meleeWeapon.SetActive(true);
        if ((inventory[selectedItem]?.GetAttributes().Type & (int) ItemType.Weapon) == (int) ItemType.Weapon)
        {
            CurrentDamage = inventory[selectedItem].GetAttributes().WeaponDamage;
        }
        else
        {
            CurrentDamage = 1;
        }

        yield return new WaitForSeconds(0.25f);
        meleeWeapon.SetActive(false);
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
    #endregion

    #region Items Behavior
    // Adds an item to the inventory in the first available slot
    private void PickUpItem(GameObject item)
    {
        ItemAttributes theThingWeWant = item.GetComponent<Pickupable>().Item;

        for (int i = 0; i < inventorySize; i++)
        {
            if (inventory[i] == null )
            {
                inventory[i] = new Item(theThingWeWant);
                Destroy(item);
                break;
            }
            else if (inventory[i].GetAttributes().Name.Equals(theThingWeWant.Name)
                && inventory[i].GetCount() < inventory[i].GetAttributes().MaxStackSize)
            {
                inventory[i].IncreaseCount();
                Destroy(item);
                break;
            }
        }
    }

    // Uses the currently selected item
    private void UseItem()
    {
        if (inventory[selectedItem] == null)
        {
            return;
        }

        Item item = inventory[selectedItem];
        // todo: make the item usage check what type of item you are using and pull the values to use from the stats
        ItemAttributes stats = inventory[selectedItem].GetAttributes();

        // WEAPONS
        if ((stats.Type & (int) ItemType.Weapon) == (int) ItemType.Weapon)
        {
            // something? attacks are handled separately
        }
        // FOODS
        if ((stats.Type & (int) ItemType.Food) == (int) ItemType.Food)
        {
            hunger += stats.FoodHungerRestore;
            if (hunger > 100)
            {
                hunger = 100;
            }
            DecrementItemCount();
        }
        // TREE CHOPS
        if ((stats.Type & (int) ItemType.TreeChop) == (int) ItemType.TreeChop)
        {
            // hit tree, remove durability
            item.SetDurability(item.GetDurability() - 1);
            if (item.GetDurability() <= 0)
            {
                DecrementItemCount();
            }
        }
    }

    // Decrements the item count of a stack, or deletes the stack if empty
    private void DecrementItemCount()
    {
        inventory[selectedItem].DecreaseCount();
        if (inventory[selectedItem].GetCount() <= 0)
        {
            inventory[selectedItem] = null;
        }
    }
    #endregion

    #region Fire Mechanics
    // Creates a fire, if possible
    private void CreateFire()
    {
        // Check fire building conditions
        if (inventory[selectedItem] == null || (inventory[selectedItem].GetAttributes().Type & (int) ItemType.Burnable) != (int) ItemType.Burnable)
        {
            return;
        }
        bool positionLegal = Physics.Raycast(fireSpawnCheck.transform.position, Vector3.down, out RaycastHit hit, 5);
        if (!positionLegal)
        {
            return;
        }

        // now we build the fire
        StartCoroutine(PlaceFireCooldown());

        currentFire = Instantiate(fireplace, hit.point, Quaternion.identity);
        currentFire.GetComponent<FireLifespan>().AddFuel(inventory[selectedItem].GetAttributes().BurnableFuelValue);
        DecrementItemCount();
    }

    // The fire building coroutine - currently pretty barebones
    private IEnumerator PlaceFireCooldown()
    {
        canPlaceFire = false;
        yield return new WaitForSeconds(5f);
        canPlaceFire = true;
    }
    #endregion

    #region On Collision/Trigger
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
    #endregion
}
