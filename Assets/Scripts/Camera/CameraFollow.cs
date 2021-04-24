using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    
    public float smoothDampTime = 2f;
    public float panRate = 5f;

    public Vector2 offset = Vector2.zero;

    private Vector2 curVel = Vector2.zero;

    private void Start()
    {
        // Idiot-proofing
        if(target == null) target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if(target != null)
        {
            Vector2 targetPos = (Vector2)target.position + offset;
            
            Vector2 panPos = Vector2.SmoothDamp(transform.position, targetPos, ref curVel, smoothDampTime, panRate);

            transform.position = new Vector3(panPos.x, panPos.y, transform.position.z);
        }
    }
}
