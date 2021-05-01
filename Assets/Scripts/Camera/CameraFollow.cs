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

    public Gradient backgroundColor;
    public float maxSkyHeight = 20;
    public float maxDepth = -100;

    public bool inStartPan = false;

    private MainMenuUI mainMenuUI;

    private void Start()
    {
        // Idiot-proofing
        // idiot-proofing-proofing 
        // if(target == null) target = GameObject.FindGameObjectWithTag("Player")?.transform;

        mainMenuUI = FindObjectOfType<MainMenuUI>();
    }

    private void Update()
    {
        float heightParam = Helpers.Remap(transform.position.y, maxDepth, maxSkyHeight, 0, 1);
        heightParam = Mathf.Clamp01(heightParam);
        Camera.main.backgroundColor = backgroundColor.Evaluate(heightParam);

        if(inStartPan && target != null)
        {
            PanTowardsTarget(Time.unscaledDeltaTime);

            float distance = Vector2.Distance(transform.position, target.position);
            if(distance < 5)
            {
                inStartPan = false;
                mainMenuUI.FinishStartup();
            }
        }
    }

    private void FixedUpdate()
    {
        if(target != null)
        {
            PanTowardsTarget(Time.fixedUnscaledDeltaTime);
        }
    }

    private void PanTowardsTarget(float deltaTime)
    {
            Vector2 targetPos = (Vector2)target.position + offset;
            Vector2 panPos = Vector2.SmoothDamp(transform.position, targetPos, ref curVel, smoothDampTime, panRate, deltaTime);
            transform.position = new Vector3(panPos.x, panPos.y, transform.position.z);
    }
}
