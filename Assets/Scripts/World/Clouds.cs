using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    public float cloudMinHeight = 0;
    public float cloudMaxHeight = 40;
    
    public int maxClouds = 25;

    public GameObject[] cloudPrefabs;

    public float cloudSpeedMin;
    public float cloudSpeedMax;

    public float cloudSpawnDistance = 30;
    public float cloudDespawnDistance = 50;

    public int cloudSpawnOnStart = 15;

    private void Start()
    {
        for(int i = 0; i < cloudSpawnOnStart; ++i)
        {
            SpawnCloudOnScreen();
        }
    }

    private void Update()
    {
        while(transform.childCount < maxClouds)
        {
            SpawnCloudOutOfSight();
        }
    }

    public void SpawnCloud(float x)
    {
        float height = Random.Range(cloudMinHeight, cloudMaxHeight);
        GameObject cloudObject = Instantiate(cloudPrefabs[Random.Range(0, cloudPrefabs.Length)], new Vector3(x, height, 0), Quaternion.identity);
        Cloud cloud = cloudObject.GetComponent<Cloud>();
        cloud.despawnDistance = cloudDespawnDistance;
        cloud.movement = Random.Range(cloudSpeedMin, cloudSpeedMax);
        cloudObject.transform.parent = transform;
    }

    private void SpawnCloudOutOfSight()
    {
        float avgSpeed = (cloudSpeedMax + cloudSpeedMin) / 2;
        float cloudSpawnX = (-Mathf.Sign(avgSpeed) * cloudSpawnDistance) + Camera.main.transform.position.x;
        SpawnCloud(cloudSpawnX);
    }

    private void SpawnCloudOnScreen()
    {
        float camX = Camera.main.transform.position.x;
        float camSize = Camera.main.orthographicSize;
        float xPos = Random.Range(-camSize, camSize) + camX;
        SpawnCloud(xPos);
    }
}
