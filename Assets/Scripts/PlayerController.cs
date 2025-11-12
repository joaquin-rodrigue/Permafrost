using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///     The controller for the player. Includes code for handling all player controls, player health
///     and hunger, player inventory code, item usage and special code relating to collision.
/// </summary>
/// <remarks>
///     Used to use GetComponentsInChildren() but I changed that decision because it's dumb.<br></br>
///     This class does a lot - it involves pretty much all code related to the player.<br></br>
///     They way this class handles Unity input is a little odd, it uses the Update loop to 
///     get input values, and uses the FixedUpdate loop to actually handle those values. That
///     means there's some inputs that, even if pressed for a single frame, the boolean for
///     whatever input you pressed stays true until the next FixedUpdate loop. It's probably
///     not efficient and I warn you it's not the standard. But its surprisingly effective.
/// </remarks>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float jumpHeight = 8f;

    [Header("Health + Hunger")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxHunger = 100;
    [SerializeField] private float invulnTime = 0.35f;
    private float health;
    private float hunger;
    private bool currentlyInvuln;

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
    private bool sprinting;
    private bool isInLight;

    // Component references
    private PlayerInput playerInput;
    private Rigidbody rb;
    private Collider collision;
    private DayNightCycle daylight;

    // Inventory
    private Item[] inventory;
    private int selectedItem = 0;
    private int inventorySize;
    private bool canUseItem = true;

    // UI
    private UIController ui;
    private TMP_Text[] inventoryItemTexts;

    [Header("Item References")]
    [SerializeField] private GameObject meleeWeapon;
    [SerializeField] private float useItemCooldown;
    [SerializeField] private GameObject personalLight;

    public int CurrentDamage { get; private set; }

    [Header("Fire References")]
    [SerializeField] private GameObject fireplace;
    [SerializeField] private GameObject fireSpawnCheck;

    private GameObject currentFire;
    private bool createFireButton;
    private bool canPlaceFire = true;

    [Header("Terrain")]
    [Tooltip("Points to check under for generating new chunks. Ensure these transforms are far above the player transform!")]
    [SerializeField] private GameObject[] terrainSpawnCheckPoints;
    [SerializeField] private LayerMask terrainLayer;

    private RandomTerrainGenerator generator;
    private float terrainUpdateCheckTimer;
    private bool chunksGenerating;

    #region Start + Update(s)
    // Getting references to necessary objects + initializing the inventory system
    void Start()
    {
        health = maxHealth;
        hunger = maxHunger;

        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        collision = GetComponent<Collider>();
        daylight = GameObject.Find("Directional Light").GetComponent<DayNightCycle>();
        ui = GameObject.Find("Canvas").GetComponent<UIController>();
        generator = GameObject.Find("TerrainGenerator").GetComponent<RandomTerrainGenerator>();

        // The getcomponentsinchildren call here is gone, instead it must be set in the editor
        meleeWeapon.SetActive(false);

        inventorySize = 5;
        inventory = new Item[inventorySize];
        //InvokeRepeating(nameof(TerrainUpdateCheck), 1, 1);
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
        sprinting = playerInput.actions["Sprint"].IsInProgress();
    }

    // Processing player inputs this frame happens here for smoother and more consistent gameplay
    // TODO: Clean this up; move some code out to separate functions
    private void FixedUpdate()
    {
        // Check if the player is dead
        if (health <= 0)
        {
            Debug.LogError("you are ded. not big surprise");
            return;
        }

        // Moving
        float targetSpeed = sprinting ? sprintSpeed : moveSpeed;
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
        hunger -= 0.003f + (sprinting ? 0.009f : 0);
        if (hunger <= 0)
        {
            hunger = 0;
            Hurt(0.05f);
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

        // Use/Update item - all the code is done in UseItem and UpdateItem
        UpdateItem();
        if (usingItem && canUseItem)
        {
            UseItem();
        }

        // Create a fire
        if (createFireButton && canPlaceFire)
        {
            CreateFire();
        }
        createFireButton = false;

        // update the UI
        ui.UpdateInventoryUI(inventory, selectedItem);
        ui.UpdateButtonPrompts(inventory[selectedItem]);
        ui.UpdateHungerUI(hunger);

        // Check terrain updating
        terrainUpdateCheckTimer += chunksGenerating ? 0 : Time.fixedDeltaTime;
        if (terrainUpdateCheckTimer > 1)
        {
            terrainUpdateCheckTimer = 0;
            TerrainUpdateCheck();
        }
    }

    /// <summary>
    /// Periodic check to generate a new chunk. Only runs up to once per second to prevent too much lag.
    /// </summary>
    private void TerrainUpdateCheck()
    {
        // Terrain generation update check
        foreach (GameObject point in terrainSpawnCheckPoints)
        {
            Transform t = point.transform;
            if (!Physics.Raycast(t.position, Vector3.down, 128, terrainLayer))
            {
                Debug.Log("generate new chunk!" + Time.frameCount);
                Vector2 temp = new(t.position.x, t.position.z);
                chunksGenerating = true;
                generator.GenerateNewChunk(temp);
                chunksGenerating = false;
            }
        }
    }
    #endregion

    #region Weapon/Hurt Behavior
    // The melee attack coroutine
    // TODO: replace this with a call for melee attack animation on the given weapon equipped
    // well some of the code will remain here but the animation wont
    private IEnumerator MeleeAttack()
    {
        canAttack = false;
        meleeWeapon.SetActive(true);
        if (inventory[selectedItem] != null && inventory[selectedItem].IsType(ItemType.Weapon))
        {
            CurrentDamage = inventory[selectedItem].Stats.WeaponDamage;
            hunger -= inventory[selectedItem].Stats.WeaponHungerLoss;
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

    // Simple way for the objects you hit to communicate back that the weapon's durability needs to drop
    public void MeleeHit()
    {
        inventory[selectedItem].DecrementDurability();
        if (inventory[selectedItem].GetDurability() <= 0)
        {
            DecrementItemCount();
        }
    }

    // Hurts the player
    public void Hurt(float damage)
    {
        if (currentlyInvuln) return;
        health -= damage;
        StartCoroutine(Invulnerability());
        //Debug.Log(health);
        ui.UpdateHealthUI(health);
    }

    private IEnumerator Invulnerability()
    {
        currentlyInvuln = true;
        yield return new WaitForSeconds(invulnTime);
        currentlyInvuln = false;
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
            else if (inventory[i].Stats.Name.Equals(theThingWeWant.Name)
                && inventory[i].GetCount() < inventory[i].Stats.MaxStackSize)
            {
                inventory[i].IncrementCount();
                Destroy(item);
                break;
            }
        }
    }

    // Used for specific item types to update their state
    private void UpdateItem()
    {
        if (inventory[selectedItem] == null) return;

        Item item = inventory[selectedItem];
        ItemAttributes stats = inventory[selectedItem].Stats;

        // LIGHTS
        if (item.IsType(ItemType.Light))
        {
            personalLight.SetActive(true);
            Light theLight = personalLight.GetComponent<Light>();
            theLight.color = stats.LightColor;
            theLight.intensity = stats.LightIntensity;
            theLight.range = stats.LightRange;
            item.DecrementDurability();
        }
        else
        {
            personalLight.SetActive(false);
        }
    }

    // Uses the currently selected item
    private void UseItem()
    {
        if (inventory[selectedItem] == null) return;

        StartCoroutine(ItemCooldown());
        Item item = inventory[selectedItem];
        ItemAttributes stats = inventory[selectedItem].Stats;

        // WEAPONS
        if (item.IsType(ItemType.Weapon))
        {
            // something? attacks are handled separately
        }
        // FOODS
        if (item.IsType(ItemType.Food))
        {
            hunger += stats.FoodHungerRestore;
            if (hunger > 100)
            {
                hunger = 100;
            }
            DecrementItemCount();
        }
        // TREE CHOPS
        if (item.IsType(ItemType.TreeChop))
        {
            // todo: do we need to tie usage here if tree chop is part of attack animations
        }
    }

    private IEnumerator ItemCooldown()
    {
        canUseItem = false;
        yield return new WaitForSeconds(useItemCooldown);
        canUseItem = true;
    }

    // Decrements the item count of a stack, or deletes the stack if empty
    private void DecrementItemCount()
    {
        inventory[selectedItem].DecrementCount();
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
        if (inventory[selectedItem] == null || !inventory[selectedItem].IsType(ItemType.Burnable)) return;
        if (!Physics.Raycast(fireSpawnCheck.transform.position, Vector3.down, out RaycastHit hit, 5)) return;

        // now we build the fire (or add fuel to an exisiting one)
        if (hit.transform.TryGetComponent(out FireLifespan fire))
        {
            fire.AddFuel(inventory[selectedItem].Stats.BurnableFuelValue);
        }
        else
        {
            StartCoroutine(PlaceFireCooldown());
            currentFire = Instantiate(fireplace, hit.point + new Vector3(0, 0.5f, 0), Quaternion.identity);
            currentFire.GetComponent<FireLifespan>().AddFuel(inventory[selectedItem].Stats.BurnableFuelValue);
        }

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

    #region Breaking Objects Behavior
    private void CheckBreakable(Breakable obj)
    {
        if (inventory[selectedItem] == null || obj.GetDurability() <= 0) return;

        if (inventory[selectedItem].IsType(obj.Stats.TypeForBreaking))
        {
            obj.DecreaseDurability(inventory[selectedItem].Stats.BreakingStrength);
            Debug.Log("Tree at durability " + obj.GetDurability());
        }

        if (obj.GetDurability() <= 0)
        {
            int dropCount = Random.Range(obj.Stats.MinDropCount, obj.Stats.MaxDropCount);
            for (int i = 0; i < dropCount; i++)
            {
                Instantiate(obj.Stats.DropItem, obj.gameObject.transform.position + new Vector3(
                    Random.Range(-1f, 1f), 1, Random.Range(-1f, 1f)
                ), Quaternion.identity);
            }
            Destroy(obj.gameObject);
        }

        inventory[selectedItem].DecrementDurability();
        if (inventory[selectedItem].GetDurability() <= 0)
        {
            DecrementItemCount();
        }
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
            // quick distance check to make sure disjointed hitboxes don't cause item pickup
            if (Vector3.Distance(transform.position, other.transform.position) < 1.5f) PickUpItem(other.gameObject);
        }
        if (other.CompareTag("Tree"))
        {
            CheckBreakable(other.GetComponent<Breakable>());
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
