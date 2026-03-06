using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

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
    // Movement
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float jumpHeight = 8f;

    [Header("Health + Hunger")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxHunger = 100;
    [SerializeField] private float invulnTime = 0.35f;
    [SerializeField] private float darknessDamageThreshold = 0.25f;
    [SerializeField] private float darknessTimeMultiplier = 0.1f;
    [SerializeField] private float starveDamage = 0.05f;
    [SerializeField] private float passiveHungerLoss = 0.003f;
    [SerializeField] private float sprintHungerLoss = 0.009f;
    private float health;
    private float hunger;
    private bool currentlyInvuln;
    private float darknessTimer;

    // Input variables
    private Vector2 moveInput;
    private bool jumpInput;
    private bool jumpHeld;
    private bool crouching;
    private bool canJump;
    private bool attacking;
    private bool canAttack = true;
    private bool usingItem;
    private bool dropItemInput;
    private bool nextItemInput;
    private bool previousItemInput;
    private bool sprinting;
    private bool isInLight;
    private bool interacting;
    private bool pauseInput;

    // Component references
    private PlayerInput playerInput;
    private Rigidbody rb;
    private Collider collision;
    private DayNightCycle daylight;

    // Inventory
    private Item[] inventory;
    private int selectedItem = 0;
    private readonly int inventorySize = 9;
    private bool canUseItem = true;

    // UI
    private UIController ui;
    public bool InteractUIActive { get; private set; }
    public bool IsPaused { get; private set; }

    // Audio stuff
    [Header("Audio Effects")]
    [SerializeField] private AudioClip[] meleeSwingAudio;
    [SerializeField] private float meleeSwingVolume;
    [SerializeField] private AudioClip[] eatSounds;
    [SerializeField] private float eatVolume;

    // Other item related
    [Header("Interactions References")]
    [SerializeField] private GameObject meleeWeapon;
    [SerializeField] private GameObject itemViewModel;
    private Animator itemViewModelAnimation;
    private AudioSource itemViewModelAudio;
    [SerializeField] private ParticleSystem itemParticles;
    [SerializeField] private float useItemCooldown;
    [SerializeField] private GameObject personalLight;
    [SerializeField] private Transform interactCheckPoint;

    public int CurrentDamage { get; private set; }

    // Fires
    [Header("Fire References")]
    [SerializeField] private GameObject fireplace;
    [SerializeField] private GameObject fireSpawnCheck;
    [SerializeField] private ItemAttributes torchStats;
    [SerializeField] private LayerMask fireBuildableLayers;

    private GameObject currentFire;
    private bool createFireButton;
    private bool canPlaceFire = true;

    // The car!!!!
    [Header("Car")]
    [SerializeField] private GameObject car;

    // Terrain
    [Header("Terrain")]
    [Tooltip("Points to check under for generating new chunks. Ensure these transforms are far above the player transform!")]
    [SerializeField] private GameObject[] terrainSpawnCheckPoints;
    [SerializeField] private LayerMask terrainLayer;

    private RandomTerrainGenerator generator;
    private float terrainUpdateCheckTimer;
    private bool chunksGenerating;

    // Saving data
    [SerializeField] private SaveData dataCollector;

    [Header("Item Drop Library")]
    [SerializeField] private ItemLibrary itemLibrary;

    #region Start + Update(s)
    // Getting references to necessary objects + initializing the inventory system
    void Start()
    {
        // health and hungy
        health = maxHealth;
        hunger = maxHunger;

        // general component things
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        collision = GetComponent<Collider>();
        daylight = GameObject.Find("Directional Light").GetComponent<DayNightCycle>();
        ui = GameObject.Find("Canvas").GetComponent<UIController>();
        generator = GameObject.Find("TerrainGenerator").GetComponent<RandomTerrainGenerator>();

        // set some stuff
        meleeWeapon.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        inventory = new Item[inventorySize];

        // item view model stuff
        ChangeViewModel(); // set the view model to turn off by default
        itemViewModelAnimation = itemViewModel.GetComponent<Animator>();
        itemViewModelAudio = itemViewModel.GetComponent<AudioSource>();
        itemParticles.Stop();
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
        dropItemInput = playerInput.actions["DropItem"].WasPressedThisFrame() || dropItemInput;
        nextItemInput = playerInput.actions["Next"].WasPressedThisFrame() || nextItemInput;
        previousItemInput = playerInput.actions["Previous"].WasPressedThisFrame() || previousItemInput;
        createFireButton = playerInput.actions["Fire"].WasPressedThisFrame() || createFireButton;
        sprinting = playerInput.actions["Sprint"].IsInProgress();
        interacting = playerInput.actions["Interact"].WasPressedThisFrame() || interacting;
        pauseInput = playerInput.actions["Pause"].IsPressed() || pauseInput;
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

        // Check if the game is paused
        if (pauseInput || IsPaused)
        {
            PauseGame();
            return;
        }
        pauseInput = false;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            InteractUIActive = false;
        }

        // Update time survived
        dataCollector.timeSurvived += Time.fixedDeltaTime;
        //Debug.Log(dataCollector.timeSurvived);

        // Moving
        float targetSpeed = sprinting ? sprintSpeed : moveSpeed;
        Vector2 targetVelocity = moveInput * targetSpeed;
        rb.linearVelocity = transform.rotation * new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.y);
        //Debug.Log("vel: " + rb.linearVelocity);
        //Debug.Log("jump vars: " + jumpInput + " " + canJump);

        // Jump 
        if (jumpInput && canJump)
        {
            Vector3 jump = new Vector3(0, jumpHeight, 0);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddRelativeForce(jump, ForceMode.Impulse);
            //Debug.Log("jumpin");
            canJump = false;
        }
        jumpInput = false;

        // Attack
        if (attacking && canAttack)
        {
            DetermineAttack();
        }
        attacking = false;

        // The darkness consumes you
        if (daylight.LightValue < darknessDamageThreshold && !isInLight)
        {
            darknessTimer += Time.deltaTime;
            Hurt(darknessDamageThreshold - daylight.LightValue + darknessTimer * darknessTimeMultiplier);
        }
        else
        {
            darknessTimer = 0;
        }

        // The hunger also consumes you
        hunger -= passiveHungerLoss + (sprinting ? sprintHungerLoss : 0);
        if (hunger <= 0)
        {
            hunger = 0;
            Hurt(starveDamage);
        }
        // Passive regeneration at high hunger
        if (hunger >= maxHunger * 0.9 && health < maxHealth)
        {
            health += 0.01f;
            hunger -= 0.02f;
        }

        // inventory management
        if (nextItemInput)
        {
            selectedItem++;
            if (selectedItem >= inventorySize)
            {
                selectedItem = 0;
            }
            ChangeViewModel();
        }
        else if (previousItemInput)
        {
            selectedItem--;
            if (selectedItem < 0)
            {
                selectedItem = inventorySize - 1;
            }
            ChangeViewModel();
        }
        nextItemInput = false;
        previousItemInput = false;
       
        // Todo: drop (probably works now)
        if (dropItemInput && canUseItem)
        {
            DropItem(false);
        }
        dropItemInput = false;

        // Use/Update item - all the code is done in UseItem and UpdateItem
        UpdateItem();
        if (usingItem && canUseItem)
        {
            UseItem();
        }
        usingItem = false;

        // Interact with something
        if (interacting)
        {
            TryInteract();
        }
        interacting = false;

        // Create a fire
        if (createFireButton && canPlaceFire)
        {
            CreateFire();
        }
        createFireButton = false;

        // update the UI
        ui.UpdateInventoryUI(inventory, selectedItem);
        ui.UpdateButtonPrompts(inventory[selectedItem], interactCheckPoint.transform);
        ui.UpdateHungerUI(hunger, maxHunger);

        // Check terrain updating
        terrainUpdateCheckTimer += chunksGenerating ? 0 : Time.fixedDeltaTime;
        if (terrainUpdateCheckTimer > 1)
        {
            terrainUpdateCheckTimer = 0;
            TerrainUpdateCheck();
        }
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    private void PauseGame()
    {
        Time.timeScale = 0f;
        IsPaused = true;
        Cursor.lockState = CursorLockMode.Confined;
        ui.ActivatePauseMenu();
    }

    /// <summary>
    /// Unpauses the game.
    /// </summary>
    public void UnpauseGame()
    {
        IsPaused = false;
        pauseInput = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        ui.DeactivatePauseMenu();
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
    // Determines which attack to do, or if to do an attack at all
    private void DetermineAttack()
    {
        if (inventory[selectedItem] == null) return;
        if (!inventory[selectedItem].IsType(ItemType.Weapon)) return;

        canAttack = false;
        CurrentDamage = inventory[selectedItem].Stats.WeaponDamage;
        hunger -= inventory[selectedItem].Stats.WeaponHungerLoss;

        switch (inventory[selectedItem].Stats.WeaponAttack)
        {
            case WeaponType.None:
                Debug.LogWarning("Weapon has no attack type!");
                canAttack = true;
                break;
            case WeaponType.Melee:
                StartCoroutine(MeleeAttack());
                break;
            case WeaponType.Gun:
                StartCoroutine(GunAttack());
                break;
        }
    }

    // The melee attack coroutine
    private IEnumerator MeleeAttack()
    {
        itemViewModelAnimation.SetBool("swinging", true);
        yield return new WaitForSeconds(0.12f);
        itemViewModelAudio.clip = meleeSwingAudio[Random.Range(0, meleeSwingAudio.Length)];
        itemViewModelAudio.volume = meleeSwingVolume;
        itemViewModelAudio.Play();
        yield return new WaitForSeconds(0.13f);
        meleeWeapon.SetActive(true);
        itemParticles.Play();
        yield return new WaitForSeconds(0.25f);
        itemViewModelAnimation.SetBool("swinging", false);
        itemParticles.Stop();
        meleeWeapon.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        canAttack = true;
    }

    // The ranged attack coroutine
    // Not yet implemented due to no weapons using ranged attacks
    private IEnumerator GunAttack()
    {
        RaycastHit hit;
        if (!Physics.Raycast(interactCheckPoint.position, interactCheckPoint.forward, out hit, 100))
        {
            canAttack = true;
            yield break;
        }
        if (inventory[selectedItem].GetCurrentAmmo() <= 0)
        {
            canAttack = true;
            yield break;
        }

        GameObject target = hit.collider.gameObject;
        // todo: create a creaturestatemachine base class so we can just try getting that component here
        yield return new WaitForEndOfFrame();
        canAttack = true;
    }

    // Simple way for the objects you hit to communicate back that the weapon's durability needs to drop
    public void OnHitSuccess()
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
        Debug.Log(damage);
        ui.UpdateHealthUI(health, maxHealth);
        if (health <= 0)
        {
            // tod: death
            IsPaused = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.Confined;
            ui.ActivateDeathMenu();
        }
    }

    // Invulnerability timer between hits
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
            if (inventory[i] == null)
            {
                inventory[i] = new Item(theThingWeWant);
                Destroy(item);
                dataCollector.itemsGathered++;
                ChangeViewModel();
                break;
            }
            else if (inventory[i].Stats.Name.Equals(theThingWeWant.Name)
                && inventory[i].GetCount() < inventory[i].Stats.MaxStackSize)
            {
                inventory[i].IncrementCount();
                Destroy(item);
                dataCollector.itemsGathered++;
                break;
            }
        }
    }

    // Used for specific item types to update their state
    private void UpdateItem()
    {
        personalLight.SetActive(false);
        isInLight = false;
        if (inventory[selectedItem] == null) return;

        Item item = inventory[selectedItem];
        ItemAttributes stats = inventory[selectedItem].Stats;

        // LIGHTS
        if (item.IsType(ItemType.Light))
        {
            personalLight.SetActive(true);
            isInLight = true;
            Light theLight = personalLight.GetComponent<Light>();
            theLight.color = stats.LightColor;
            theLight.intensity = stats.LightIntensity;
            theLight.range = stats.LightRange;
            item.DecrementDurability();
            ChangeViewModel();
        }
    }

    // Uses the currently selected item
    private void UseItem()
    {
        if (inventory[selectedItem] == null) return;
        //Debug.Log("using item!");
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
            if (hunger > maxHunger)
            {
                hunger = maxHunger;
            }
            itemViewModelAudio.clip = eatSounds[Random.Range(0, eatSounds.Length)];
            itemViewModelAudio.volume = eatVolume;
            itemViewModelAudio.Play();
            DecrementItemCount();
        }
        // TREE CHOPS
        if (item.IsType(ItemType.TreeChop))
        {
            // todo: do we need to tie usage here if tree chop is part of attack animations
        }
        // HEALS
        if (item.IsType(ItemType.Heal))
        {
            health += stats.HealthRestore;
            if (health > maxHealth)
            {
                health = maxHealth;
            }
            DecrementItemCount();
        }
    }

    // Cooldown between using items (because I managed to turbo chug 4 foods in 4 frames one time)
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
            ChangeViewModel();
        }
    }

    // Drops the currently held item
    // Unused option to drop the entire item stack instead
    private void DropItem(bool stack)
    {
        if (inventory[selectedItem] == null) return;

        StartCoroutine(ItemCooldown());
        do
        {
            itemLibrary.CreatePhysicalItem(inventory[selectedItem].Stats.Name, transform);
            DecrementItemCount();
        } 
        while (stack && inventory[selectedItem].GetCount() > 0);
        ChangeViewModel();
    }

    // Changes the view model seen on the screen
    private void ChangeViewModel()
    {
        if (inventory[selectedItem] == null)
        {
            itemViewModel.SetActive(false);
            return;
        }

        string name = inventory[selectedItem].Stats.Name;
        Mesh mesh = itemLibrary.GetItemModel(name);
        Material mat = itemLibrary.GetItemMaterial(name);
        Vector3 scale = itemLibrary.GetItemScale(name);
        Quaternion rot = itemLibrary.GetItemRotation(name);
        if (mesh == null || mat == null) // scale and rotation won't be null coming out of the item lib
        {
            itemViewModel.SetActive(false);
            return;
        }

        itemViewModel.GetComponent<MeshRenderer>().material = mat;
        itemViewModel.GetComponent<MeshFilter>().mesh = mesh;
        itemViewModel.transform.localScale = scale;
        itemViewModel.transform.rotation = rot;
        itemViewModel.SetActive(true);
        itemParticles.Stop();
    }
    #endregion

    #region Interact Button Stuff
    // Raycasts in front of you; determines what item to interact with in front of you
    private void TryInteract()
    {
        RaycastHit hit;
        if (!Physics.Raycast(interactCheckPoint.transform.position, interactCheckPoint.forward, out hit, 5))
        {
            ui.SetErrorText("Nothing important here...");
            return;
        }
        if (hit.collider.TryGetComponent(out FireLifespan fire))
        {
            ui.OpenFireplaceUI(fire);
            currentFire = fire.gameObject;
            InteractUIActive = true;
            return;
        }
        if (hit.collider.TryGetComponent(out TheCar theCar))
        {
            ui.OpenCarUI(theCar);
            InteractUIActive = true;
            return;
        }
        ui.SetErrorText("Nothing important here...");
    }
    #endregion

    #region Fire Mechanics
    // Creates a fire, if possible
    private void CreateFire()
    {
        // Check fire building conditions
        if (inventory[selectedItem] == null || !inventory[selectedItem].IsType(ItemType.Burnable))
        {
            ui.SetErrorText("This item won't burn.");
            return;
        }
        if (!Physics.Raycast(fireSpawnCheck.transform.position, Vector3.down, out RaycastHit hit, 5, fireBuildableLayers))
        {
            ui.SetErrorText("I need to find flatter ground...");
            return;
        }
        Debug.Log("fire: " + hit.point);
        Debug.Log("fire: " + hit.collider.gameObject.name);
        // now we build the fire 
        StartCoroutine(PlaceFireCooldown());
        currentFire = Instantiate(fireplace, hit.point + new Vector3(0, 0.5f, 0), Quaternion.identity);
        currentFire.GetComponent<FireLifespan>().AddFuel(inventory[selectedItem].Stats.BurnableFuelValue);
        dataCollector.firesBuilt++;
        DecrementItemCount();
    }

    // The fire building coroutine - currently pretty barebones
    private IEnumerator PlaceFireCooldown()
    {
        canPlaceFire = false;
        yield return new WaitForSeconds(5f);
        canPlaceFire = true;
    }

    // Adds fuel to a fire you have selected
    public void AddFuelToSelectedFire()
    {
        if (currentFire == null)
        {
            ui.SetErrorText("I'm not even at a fire, how am I doing this?");
            return;
        }
        Item item = inventory[selectedItem];
        if (item == null || !item.IsType(ItemType.Burnable))
        {
            ui.SetErrorText("This item won't burn well");
            return;
        }
        FireLifespan lifespan = currentFire.GetComponent<FireLifespan>();
        lifespan.AddFuel(item.Stats.BurnableFuelValue);
        ui.OpenFireplaceUI(lifespan);
        DecrementItemCount();
    }

    // Creates a torch if you have the wood/inventory space to do so
    public void CreateTorch()
    {
        if (currentFire == null)
        {
            ui.SetErrorText("I'm not even at a fire, how am I doing this?");
            return;
        }
        Item item = inventory[selectedItem];
        if (item == null || !item.IsType(ItemType.Burnable))
        {
            ui.SetErrorText("This item won't burn well");
            return;
        }
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventory[i] == null)
            {
                Item theTorch = new Item(torchStats);
                inventory[i] = theTorch;
                DecrementItemCount();
                return;
            }
        }
        ui.SetErrorText("My inventory doesn't have space for a torch...");
    }
    #endregion

    #region The Finish Line Car
    // Adds fuel to the car if you're holding it
    public void AddGasToCar()
    {
        Item item = inventory[selectedItem];
        if (item == null || item.Stats.Name != "Gas Can")
        {
            ui.SetErrorText("This item isn't car fuel...");
            return;
        }
        TheCar carstats = car.GetComponent<TheCar>();
        carstats.AddAFuel();
        ui.OpenCarUI(carstats);
        DecrementItemCount();
    }

    // THE WIN CONDITION
    public void ESCAPE()
    {
        int fuel = car.GetComponent<TheCar>().FuelCount();
        if (fuel < 10)
        {
            ui.SetErrorText("I need more fuel to get out of here...");
            return;
        }
        Debug.LogError("YOU WIN!!!");
        ui.SetErrorText("You win! Cutscene will exist later though sorry");
    }
    #endregion

    #region Breaking Objects Behavior
    // Checks what kind of breakable object this is and whether we are holding the right item to break it with
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

    // Another check to detect if the player is in a light source
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Light Source"))
        {
            isInLight = true;
        }
    }

    // Check for when the player leaves a light source
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Light Source"))
        {
            isInLight = false;
        }
    }
    #endregion
}
