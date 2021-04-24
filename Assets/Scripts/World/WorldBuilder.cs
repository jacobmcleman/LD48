using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldBuilder : MonoBehaviour
{
    private Tilemap terrainTiles;

    public int xBounds = 100;
    public int depthLimit = 100;

    public int waterLevel = 0;

    public int minDepth = 20;


    public float sandThickAvg = 4;
    public float sandThickVariance = 4;
    public Tile sandTile;
    public Tile rockTile;

    public Tile goldTile;

    private void Start()
    {
        terrainTiles = transform.Find("Terrain").GetComponent<Tilemap>();

        

        for(int x = -xBounds; x < xBounds; ++x)
        {
            float heightSamplePos = (x - xBounds) / 40.0f;

            float hilliness = Mathf.PerlinNoise(heightSamplePos, -1.0f);
            float height = Helpers.Remap(hilliness * Mathf.PerlinNoise(heightSamplePos, 0.0f), 0, 1, -minDepth, -depthLimit);
            float sandThickness = Helpers.Remap(Mathf.PerlinNoise(heightSamplePos, 0.5f), 0, 1, sandThickAvg + sandThickVariance, sandThickAvg - sandThickVariance);

            for(int y = -depthLimit; y < height; ++y)
            {
                float oreSampleXpos = (x - xBounds) / 35.0f;
                float oreSampleYpos = y / 20.0f;
                float ore = Mathf.PerlinNoise(oreSampleXpos, oreSampleYpos);
                //ore *= ore;

                Tile toPlace = y < (height - sandThickness) ? rockTile : sandTile;

                if(ore > 0.7f && Random.Range(0.7f, 1f) < ore)
                {
                    toPlace = goldTile;
                }

                Vector3Int tilePos = new Vector3Int(x, y, 0);
                terrainTiles.SetTile(tilePos, toPlace);
            }
        }
    }
}
