using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public float despawnDistance;

    public float movement;
    
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        
        if(distance > despawnDistance) Destroy(gameObject);
        else
        {
            transform.position = transform.position + new Vector3(movement * Time.unscaledDeltaTime, 0, 0);
        }

    }
}
