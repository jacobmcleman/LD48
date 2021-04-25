using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    public float timeToLive = 5.0f;

    private void Start()
    {
        Destroy(gameObject, timeToLive);
    }
}
