using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterFlow : MonoBehaviour
{
    private Tilemap waterTiles;
    private Tilemap terrainTiles;

    public Tile waterTile;
    public Tile waterSurfaceTile;

    public Tile[] waterloggableTiles;

    private Queue<Vector3Int> toUpdateNextFrame;

    public int xBounds = 100;
    public int depthLimit = 100;

    public int waterLevel = 0;

    private void Awake()
    {
        toUpdateNextFrame = new Queue<Vector3Int>();
    }

    private void Start()
    {
        waterTiles = transform.Find("Ocean").GetComponent<Tilemap>();
        terrainTiles = transform.Find("Terrain").GetComponent<Tilemap>();

        for(int i = -xBounds; i < xBounds; ++i)
        {
            Vector3Int tilePos = new Vector3Int(i, waterLevel, 0);
            if(isWaterLoggable(terrainTiles.GetTile(tilePos)))
            {
                waterTiles.SetTile(tilePos, waterSurfaceTile);
                toUpdateNextFrame.Enqueue(tilePos);
            }
        }
    }

    private void Update()
    {
        Queue<Vector3Int> updating = new Queue<Vector3Int>(toUpdateNextFrame);
        toUpdateNextFrame = new Queue<Vector3Int>();

        while(updating.Count > 0)
        {
            UpdateWaterTile(updating.Dequeue());
        }
    }

    private void RegisterWaterChange(Vector3Int tilePos)
    {
        toUpdateNextFrame.Enqueue(tilePos);
        toUpdateNextFrame.Enqueue(tilePos + new Vector3Int(0, -1, 0));
        toUpdateNextFrame.Enqueue(tilePos + new Vector3Int(1, 0, 0));
        toUpdateNextFrame.Enqueue(tilePos + new Vector3Int(-1, 0, 0));
    }

    public void RegisterTerrainChange(Vector3Int tilePos)
    {
        toUpdateNextFrame.Enqueue(tilePos);
        toUpdateNextFrame.Enqueue(tilePos + new Vector3Int(0, 1, 0));
        toUpdateNextFrame.Enqueue(tilePos + new Vector3Int(1, 0, 0));
        toUpdateNextFrame.Enqueue(tilePos + new Vector3Int(-1, 0, 0));
    }

    private bool TryPlaceWater(Vector3Int tilePos)
    {
        if(tilePos.x > xBounds || tilePos.x < -xBounds || tilePos.y < -depthLimit) return false;
        if(waterTiles.GetTile(tilePos) != null) return false;
        if(!isWaterLoggable(terrainTiles.GetTile(tilePos))) return false;

        waterTiles.SetTile(tilePos, waterTile);

        RegisterWaterChange(tilePos);

        return true;
    }

    public bool isWaterLoggable(TileBase tile)
    {
        if(tile == null) return true;

        foreach(Tile waterloggable in waterloggableTiles)
        {
            if(tile == waterloggable) return true;
        }

        return false;
    }

    private void UpdateWaterTile(Vector3Int tilePos)
    {
        if(waterTiles.GetTile(tilePos) == null) return;

        if(!isWaterLoggable(terrainTiles.GetTile(tilePos)))
        {
            waterTiles.SetTile(tilePos, null);

            RegisterWaterChange(tilePos);
        }

        bool didSpread = true;

        if(!TryPlaceWater(tilePos + new Vector3Int(0, -1, 0)))
        {
            if(!TryPlaceWater(tilePos + new Vector3Int(1, 0, 0)))
            {
                didSpread = TryPlaceWater(tilePos + new Vector3Int(-1, 0, 0));
            }
        }

        if(didSpread)
        {
            toUpdateNextFrame.Enqueue(tilePos);
        }
    }
}
