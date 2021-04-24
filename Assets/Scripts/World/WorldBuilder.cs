using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldBuilder : MonoBehaviour
{
    private Tilemap terrainTiles;

    public int seed = 0;
    private int noiseOffsetX = 0;
    private int noiseOffsetY = 0;

    public int xBounds = 100;
    public int depthLimit = 100;

    public int waterLevel = 0;

    public int minDepth = 20;


    public float sandThickAvg = 4;
    public float sandThickVariance = 4;
    public Tile sandTile;
    public Tile rockTile;

    public Tile goldTile;
    public GameObject goldNuggetPrefab;

    private int minExtent;
    private int maxExtent;

    private WaterFlow flower;

    private void Start()
    {
        terrainTiles = transform.Find("Terrain").GetComponent<Tilemap>();
        flower = GetComponent<WaterFlow>();

        if(seed == 0) seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        noiseOffsetX = Random.Range(int.MinValue, int.MaxValue);
        noiseOffsetY = Random.Range(int.MinValue, int.MaxValue);

        
        minExtent = -20;
        maxExtent = 20;
        for(int x = minExtent; x < maxExtent; ++x)
        {
            GenerateSlice(x);
        }

        if(goldNuggetPrefab == null)
        {
            Debug.LogWarning("No prefab assigned for gold nugget!");
        }
    }

    private void GenerateSlice(int x)
    {
        float heightSamplePos =  (x - xBounds) / 40.0f;

        float hilliness = Mathf.PerlinNoise(heightSamplePos, noiseOffsetY - 1.0f);
        float height = Helpers.Remap(hilliness * Mathf.PerlinNoise(heightSamplePos, noiseOffsetY), 0, 1, -minDepth, -depthLimit);
        float sandThickness = Helpers.Remap(Mathf.PerlinNoise(heightSamplePos, noiseOffsetY + 0.5f), 1, 0, sandThickAvg + sandThickVariance, sandThickAvg - sandThickVariance);

        for(int y = -depthLimit; y < height; ++y)
        {
            float oreSampleXpos = (x - xBounds) / 35.0f;
            float oreSampleYpos = (y) / 20.0f;
            float ore = Mathf.PerlinNoise(oreSampleXpos, oreSampleYpos);

            Tile toPlace = y < (height - sandThickness) ? rockTile : sandTile;

            if(ore > 0.7f && Random.Range(0.7f, 1f) < ore)
            {
                toPlace = goldTile;
            }

            Vector3Int tilePos = new Vector3Int(x, y, 0);
            terrainTiles.SetTile(tilePos, toPlace);
        }

        flower.AddSlice(x);

        if(x > maxExtent) maxExtent = x;
        else if(x < minExtent) minExtent = x;
    }

    private void Update()
    {
        Vector3 camPos = Camera.main.transform.position;
        int camX = (int)camPos.x;

        if(camX + 20 > maxExtent && maxExtent < xBounds) 
        {
            for(int x = maxExtent; x < camX + 20 && x < xBounds; ++x)
            {
                GenerateSlice(x);
            }
        }

        if(camX - 20 < minExtent && minExtent > -xBounds) 
        {
            for(int x = minExtent - 1; x > camX - 20 && x > -xBounds; --x)
            {
                GenerateSlice(x);
            }
        }
    }

    private void SpawnDrops(TileBase brokenType, Vector3Int tilePos)
    {
        if(brokenType == null) return;

        Tile brokenTile = (Tile)brokenType;

        GameObject toSpawn = null;

        if(brokenTile == goldTile)
        {
            toSpawn = goldNuggetPrefab;
        }

        if(toSpawn != null)
        {
            Vector3 position = terrainTiles.CellToWorld(tilePos);
            GameObject drop = Instantiate(toSpawn, position, Quaternion.identity);
        }
    }

    
    public TileBase Dig(Vector3 worldPos)
    {
        Vector3Int tilePos = terrainTiles.WorldToCell(worldPos);
        TileBase present = terrainTiles.GetTile(tilePos);

        if(present != null)
        {
            terrainTiles.SetTile(tilePos, null);
            flower.RegisterTerrainChange(tilePos);

            SpawnDrops(present, tilePos);
        }

        return present;
    }
}
