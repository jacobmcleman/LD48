
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool submerged;
    private bool headSubmerged;

    private bool onLadder;

    private PlayerInput inputSystem;

    private CameraFollow camControl;

    private CharacterController2D controller2D;
    private new Rigidbody2D rigidbody2D;
    public float waterGravityScale = 0.3f;

    public float waterDrag = 0.8f;

    public float airMoveSpeed = 3;

    public float waterMoveSpeed = 1;

    public float WaterMoveSpeed 
    {
        get { return waterMoveSpeed + UpgradeManager.SwimSpeedIncrease; }
    }

    private Vector2 moveInput;
    private Vector2 lookInput;

    private Vector3 mousePosition;

    private GameObject cursor;
    private GameObject trueCursor;
    private GameObject tileIndicator;
    private GameObject breakTileIndicator;

    private WorldBuilder builder;

    private MainMenuUI menuUI;

    public bool inUpgradeScreen;

    private GameObject upgradeScreen;

    public float digSpeed = 0.5f;
    public float slowDig = 0.25f;
    public float DigSpeed 
    {
        get { return drillCurFuel > 0 ? digSpeed + UpgradeManager.DigSpeedIncrease : SlowDig; }
    }

    public float SlowDig 
    {
        get { return slowDig + UpgradeManager.SadDigIncrease; }
    }

    public float digRadius = 5;
    public float DigRange 
    {
        get { return digRadius + UpgradeManager.DigRangeIncrease; }
    }

    public float drillBaseFuel = 20;
    private float drillCurFuel;
    public float DrillMaxFuel
    {
        get { return drillBaseFuel + UpgradeManager.DrillFuelIncrease; }
    }

    public float DrillCurrentFuel
    {
        get { return drillCurFuel; }
        set 
        { 
            drillCurFuel = value; 
            fuelMeter.maxVal = DrillMaxFuel;
            fuelMeter.Value = drillCurFuel; 
        }
    }

    public float airAmount = 10;

    public float AirCapacity 
    {
        get { return airAmount + UpgradeManager.AirCapacityIncrease; }
    }

    public float airRecoveryRate = 3;

    public float AirRecoveryRate
    {
        get { return airRecoveryRate + UpgradeManager.AirCapacityIncrease; }
    }
    public float airDecayRate = 1;
    private float curAir;

    public float healthAmount = 10;
    public float healthRecoveryRate = 0.05f;
    public float healthDecayRate = 1;
    private float curHealth;

    private Meter airMeter;
    private Meter healthMeter;
    private Meter fuelMeter;

    public AudioClip[] bubbleSounds;
    public AudioClip[] oofSounds;
    public AudioClip[] drillSounds;
    public AudioClip[] sadDigSounds;

    public AudioSource oofSource;
    public AudioSource bubbleSource;
    public AudioSource drillSource;

    public float damageSoundInterval = 1.5f;
    public float lastDamageSoundTime;

    public bool isDead;

    private Transform head;

    private GameObject deathUI;

    private Vector3 spawnPosition;

    public float deathDepth = -120f;

    private bool fireHeld;

    private Vector3Int currentlyDigging;
    private float currentTileDigTime;

    public Sprite idleSprite;
    public Sprite moveSprite;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        submerged = false;
        inUpgradeScreen = false;
        onLadder = false;
        fireHeld = false;

        controller2D = GetComponent<CharacterController2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        curAir = airAmount;
        curHealth = healthAmount;

        lastDamageSoundTime = Time.time;

        spawnPosition = transform.position;

        drillCurFuel = drillBaseFuel;
    }

    private void Start()
    {
        cursor = transform.Find("Cursor").gameObject;
        trueCursor = transform.Find("TrueCursor").gameObject;
        tileIndicator = transform.Find("TileIndicator").gameObject;
        breakTileIndicator = tileIndicator.transform.Find("BreakAnimation").gameObject;

        menuUI = FindObjectOfType<MainMenuUI>();

        head = transform.Find("Head");

        airMeter = GameObject.Find("AirBar").GetComponent<Meter>();
        airMeter.maxVal = AirCapacity;
        airMeter.minVal = 0;
        airMeter.Value = curAir;

        healthMeter = GameObject.Find("HealthBar").GetComponent<Meter>();
        healthMeter.maxVal = healthAmount;
        healthMeter.minVal = 0;
        healthMeter.Value = curHealth;

        fuelMeter = GameObject.Find("FuelBar").GetComponent<Meter>();
        fuelMeter.maxVal = DrillMaxFuel;
        fuelMeter.minVal = 0;
        fuelMeter.Value = drillCurFuel;

        builder = FindObjectOfType<WorldBuilder>();

        deathUI = GameObject.Find("DeathMessage");
        deathUI.SetActive(false);

        upgradeScreen = GameObject.Find("Upgrade Screen");
        upgradeScreen.SetActive(false);

        camControl = Camera.main.GetComponent<CameraFollow>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnPause(InputValue value)
    {
        if(menuUI.paused && menuUI.hasStarted) 
        {
            menuUI.StartResumeGame();
        }
        else if (menuUI.hasStarted)
        {
            menuUI.PauseGame();
        }
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnMousePosition(InputValue value)
    {
        mousePosition = Camera.main.ScreenToWorldPoint(value.Get<Vector2>());
    }

    public void OnFire(InputValue value)
    {
        fireHeld = value.Get<float>() == 1;
    }

    public void AddAir(float amount)
    {
        curAir += amount;
    }

    private void Update()
    {
        if(Keyboard.current[Key.F1].wasPressedThisFrame)
        {
            string path = Application.persistentDataPath;
            path += "/Screenshot_";
            path += System.DateTime.Now.Ticks;
            path += ".png";
            ScreenCapture.CaptureScreenshot(path);
        }

        if(isDead || menuUI.paused)
        {
            return;
        }

        if(transform.position.y < deathDepth)
        {
            DoDeath("Delved too deep");
            return;
        }

        if(Mathf.Abs(moveInput.x) > 0.1f)
        {
            spriteRenderer.sprite = moveSprite;
        }
        else 
        {
            spriteRenderer.sprite = idleSprite;
        }
        
        float curMoveSpeed = submerged ? WaterMoveSpeed : airMoveSpeed;
        Vector2 moveVec = new Vector2(moveInput.x * curMoveSpeed, (submerged || onLadder) ? moveInput.y * curMoveSpeed : 0);

        controller2D.Move(moveVec, false, (!submerged && !onLadder && moveInput.y > 0.1f));

        bool usingGamepad = lookInput.magnitude > 0.1f;
        Vector2 lookVec = lookInput * DigRange;
        if(!usingGamepad)
        {
            Vector2 relativeMouse = mousePosition - transform.position;
            if(relativeMouse.magnitude > DigRange) lookVec = relativeMouse.normalized * DigRange;
            else lookVec = relativeMouse;

            trueCursor.transform.localPosition = relativeMouse;
        }
        else
        {
            trueCursor.transform.localPosition = lookVec;
        }
        
        //camControl.offset = lookVec;
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookVec, DigRange, 8);
        if(hit.collider == null) 
        {
            tileIndicator.SetActive(false);
            //cursor.SetActive(true);
            cursor.transform.localPosition = lookVec;
        }
        else
        {
            cursor.transform.position = hit.point + (hit.normal * -0.1f);

            tileIndicator.SetActive(true);
            //cursor.SetActive(false);
            tileIndicator.transform.position = builder.SnapToTile(cursor.transform.position) + new Vector3(0.5f, 0.5f, 0f);
        }

        if(fireHeld)
        {
            Vector3Int currentAim = builder.SnapToGrid(cursor.transform.position);
            if(currentAim != currentlyDigging)
            {
                currentlyDigging = currentAim;
                currentTileDigTime = 0;
            }

            currentTileDigTime += Time.deltaTime;

            if(currentTileDigTime >= (1 / DigSpeed))
            {
                float fuelCost = builder.Dig(cursor.transform.position) == WorldBuilder.TileType.Sand ? 0.25f : 1.0f;
                drillCurFuel -= fuelCost;
                if(drillCurFuel < 0) drillCurFuel = 0;

                fuelMeter.maxVal = DrillMaxFuel;
                fuelMeter.Value = drillCurFuel;
                currentTileDigTime = 0;
            }

            if(builder.TilePresent(cursor.transform.position))
            {
                if(!breakTileIndicator.activeSelf)
                {
                    breakTileIndicator.SetActive(true);
                    if(!drillSource.isPlaying) 
                    {
                        if(drillCurFuel > 0) drillSource.PlayOneShot(drillSounds[Random.Range(0, drillSounds.Length)]);
                        else drillSource.PlayOneShot(sadDigSounds[Random.Range(0, sadDigSounds.Length)]);
                    }
                    
                }
                
                Meter indicator = breakTileIndicator.GetComponent<Meter>();
                indicator.maxVal = (1 / DigSpeed);
                indicator.Value = currentTileDigTime;
            }
            else
            {
                breakTileIndicator.SetActive(false);
            }
        }
        else
        {
            breakTileIndicator.SetActive(false);
            currentTileDigTime = 0;
        }


        if(headSubmerged)
        {
            curAir -= Time.deltaTime * airDecayRate;
        }
        else
        {
            curAir += Time.deltaTime * AirRecoveryRate;
        }
        curAir = Mathf.Clamp(curAir, 0, AirCapacity);
        airMeter.maxVal = AirCapacity;
        airMeter.Value = curAir;

        if(curAir == 0)
        {
            curHealth -= Time.deltaTime * healthDecayRate;

            if(Time.time - lastDamageSoundTime > damageSoundInterval)
            {
                lastDamageSoundTime = Time.time;
                bubbleSource.PlayOneShot(bubbleSounds[Random.Range(0, bubbleSounds.Length)]);
                oofSource.PlayOneShot(oofSounds[Random.Range(0, oofSounds.Length)]);
            }
        }
        else
        {
            curHealth += Time.deltaTime * healthRecoveryRate;
        }
        curHealth = Mathf.Clamp(curHealth, 0, healthAmount);

        if(curHealth == 0)
        {
            DoDeath("You ran out of air");
            GetComponent<Inventory>()?.DropAll();
        }

        healthMeter.Value = curHealth;
    }

    private void FixedUpdate()
    {
        Vector3 headPos = head.position;
        headSubmerged = !builder.IsBreathable(headPos);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Water") 
        {
            submerged = true;
            rigidbody2D.drag = waterDrag;
            rigidbody2D.gravityScale = waterGravityScale;
            rigidbody2D.velocity *= 0.1f;
        }
        else if(other.gameObject.tag == "Ladder")
        {
            onLadder = true;
        }
        else if(other.gameObject.tag == "UpgradeArea")
        {
            inUpgradeScreen = true;
            upgradeScreen.SetActive(true);

            FindObjectOfType<SaveStateManager>().TriggerSave();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == "Water") 
        {
            submerged = false;
            rigidbody2D.drag = 0;
            rigidbody2D.gravityScale = 1;
        }
        else if(other.gameObject.tag == "Ladder")
        {
            onLadder = false;
        }
        else if(other.gameObject.tag == "UpgradeArea")
        {
            inUpgradeScreen = false;
            upgradeScreen.SetActive(false);
        }
    }

    public void DoDeath(string cause)
    {
        isDead = true;
        deathUI.transform.Find("Cause of Death").GetComponent<UnityEngine.UI.Text>().text = cause;
        deathUI.SetActive(true);
    }

    public void Respawn()
    {
        Debug.Log("RESPAWNED");
        deathUI.SetActive(false);
        isDead = false;

        curAir = AirCapacity;
        curHealth = healthAmount;
        transform.position = spawnPosition;
    }

    public bool NeedFuel()
    {
        return drillCurFuel < DrillMaxFuel;
    }

    public void Refuel()
    {
        DrillCurrentFuel = DrillMaxFuel;
    }
}
