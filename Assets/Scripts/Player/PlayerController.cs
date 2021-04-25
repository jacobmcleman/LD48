
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

    private Vector2 moveInput;
    private Vector2 lookInput;

    private Vector3 mousePosition;

    private GameObject cursor;
    private GameObject trueCursor;
    private GameObject tileIndicator;

    private WorldBuilder builder;

    public float digRadius = 5;

    public float airAmount = 10;
    public float airRecoveryRate = 3;
    public float airDecayRate = 1;
    private float curAir;

    public float healthAmount = 10;
    public float healthRecoveryRate = 0.05f;
    public float healthDecayRate = 1;
    private float curHealth;

    private Meter airMeter;
    private Meter healthMeter;

    public AudioClip[] bubbleSounds;
    public AudioClip[] oofSounds;

    public AudioSource oofSource;
    public AudioSource bubbleSource;

    public float damageSoundInterval = 1.5f;
    public float lastDamageSoundTime;

    public bool isDead;

    private Transform head;

    private GameObject deathUI;

    private Vector3 spawnPosition;

    public float deathDepth = -120f;

    private void Awake()
    {
        submerged = false;

        controller2D = GetComponent<CharacterController2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();

        curAir = airAmount;
        curHealth = healthAmount;

        lastDamageSoundTime = Time.time;

        spawnPosition = transform.position;
    }

    private void Start()
    {
        cursor = transform.Find("Cursor").gameObject;
        trueCursor = transform.Find("TrueCursor").gameObject;
        tileIndicator = transform.Find("TileIndicator").gameObject;

        head = transform.Find("Head");

        airMeter = GameObject.Find("AirBar").GetComponent<Meter>();
        airMeter.maxVal = airAmount;
        airMeter.minVal = 0;
        airMeter.Value = curAir;

        healthMeter = GameObject.Find("HealthBar").GetComponent<Meter>();
        healthMeter.maxVal = healthAmount;
        healthMeter.minVal = 0;
        healthMeter.Value = curHealth;

        builder = FindObjectOfType<WorldBuilder>();

        deathUI = GameObject.Find("DeathMessage");
        deathUI.SetActive(false);

        camControl = Camera.main.GetComponent<CameraFollow>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
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
        if(!isDead) builder.Dig(cursor.transform.position);
    }

    public void AddAir(float amount)
    {
        curAir += amount;
    }

    private void Update()
    {
        if(isDead)
        {
            return;
        }

        if(transform.position.y < deathDepth)
        {
            DoDeath("Delved too deep");
            return;
        }
        
        float curMoveSpeed = submerged ? waterMoveSpeed : airMoveSpeed;
        Vector2 moveVec = new Vector2(moveInput.x * curMoveSpeed, (submerged || onLadder) ? moveInput.y * curMoveSpeed : 0);

        controller2D.Move(moveVec, false, (!submerged && !onLadder && moveInput.y > 0.1f));

        bool usingGamepad = lookInput.magnitude > 0.1f;
        Vector2 lookVec = lookInput * digRadius;
        if(!usingGamepad)
        {
            Vector2 relativeMouse = mousePosition - transform.position;
            if(relativeMouse.magnitude > digRadius) lookVec = relativeMouse.normalized * digRadius;
            else lookVec = relativeMouse;

            trueCursor.transform.localPosition = relativeMouse;
        }
        else
        {
            trueCursor.transform.localPosition = lookVec;
        }
        
        //camControl.offset = lookVec;
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookVec, digRadius, 8);
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

        if(headSubmerged)
        {
            curAir -= Time.deltaTime * airDecayRate;
        }
        else
        {
            curAir += Time.deltaTime * airRecoveryRate;
        }
        curAir = Mathf.Clamp(curAir, 0, airAmount);
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

        curAir = airAmount;
        curHealth = healthAmount;
        transform.position = spawnPosition;
    }
}
