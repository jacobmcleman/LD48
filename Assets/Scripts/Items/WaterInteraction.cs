using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterInteraction : MonoBehaviour
{
    private bool submerged;
    private new Rigidbody2D rigidbody2D;

    public float waterGravityScale = 0.2f;

    public float waterDrag = 0.8f;

    public bool requiresWater = false;

    private void Awake()
    {
        submerged = false;
        rigidbody2D = GetComponent<Rigidbody2D>();
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

            if(requiresWater)
            {
                Destroy(gameObject);
            }
        }
    }
}
