
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool submerged;

    private PlayerInput inputSystem;

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

    private void Awake()
    {
        submerged = false;

        controller2D = GetComponent<CharacterController2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        cursor = transform.Find("Cursor").gameObject;
        trueCursor = transform.Find("TrueCursor").gameObject;
        tileIndicator = transform.Find("TileIndicator").gameObject;

        builder = FindObjectOfType<WorldBuilder>();
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
        builder.Dig(cursor.transform.position);
    }

    private void Update()
    {
        float curMoveSpeed = submerged ? waterMoveSpeed : airMoveSpeed;
        Vector2 moveVec = new Vector2(moveInput.x * curMoveSpeed, submerged ? moveInput.y * curMoveSpeed : 0);

        controller2D.Move(moveVec, false, (!submerged && moveInput.y > 0.1f));

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
        
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookVec, digRadius, 8);
        if(hit.collider == null) 
        {
            tileIndicator.SetActive(false);
            cursor.transform.localPosition = lookVec;
        }
        else
        {
            cursor.transform.position = hit.point + (hit.normal * -0.1f);

            tileIndicator.SetActive(true);
            tileIndicator.transform.position = builder.SnapToTile(cursor.transform.position) + new Vector3(0.5f, 0.5f, 0f);
        }
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
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == "Water") 
        {
            submerged = false;
            rigidbody2D.drag = 0;
            rigidbody2D.gravityScale = 1;
        }
    }
}
