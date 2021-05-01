using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class WorldBuilder : MonoBehaviour
{
    public enum TileType
    {
        Air,
        Sand,
        Stone,
        GoldOre, 
        CopperOre, 
        IronOre
    }

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
    public Tile ironTile;
    public Tile copperTile;
    public GameObject goldNuggetPrefab;
    public GameObject ironNuggetPrefab;
    public GameObject copperNuggetPrefab;

    public GameObject stoneBreakEffectPrefab;

    public GameObject[] randomDebris;

    private int minExtent;
    private int maxExtent;

    public int MinBound
    {
        get => minExtent;
    }

    public int MaxBound
    {
        get => maxExtent;
    }

    private WaterFlow flower;

    public float passiveNuggetSpawnInterval = 1f;
    private float lastNuggetSpawnTime;
    public int maxPassiveNuggets = 10;
    public int minSpawnDistance = 12;
    public int maxSpawnDistance = 25;
    private LinkedList<GameObject> passiveNuggets;

    private Transform passiveSpawns;
    private Transform looseItems;

    private void Start()
    {
        terrainTiles = transform.Find("Terrain").GetComponent<Tilemap>();
        flower = GetComponent<WaterFlow>();

        if(goldNuggetPrefab == null)
        {
            Debug.LogWarning("No prefab assigned for gold nugget!");
        }

        lastNuggetSpawnTime = Time.time;

        passiveNuggets = new LinkedList<GameObject>();

        passiveSpawns = transform.Find("Passives");
        looseItems = GameObject.Find("LooseItems").transform;
    }
    
    public void BuildWorld()
    {
        if(seed == 0) 
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            Random.InitState(seed);

            noiseOffsetX = Random.Range(int.MinValue, int.MaxValue);
            noiseOffsetY = Random.Range(int.MinValue, int.MaxValue);

            minExtent = -30;
            maxExtent = 30;
            for(int x = minExtent; x <= maxExtent; ++x)
            {
                GenerateSlice(x);
            }
        }
        else
        {
            Debug.LogFormat("Loading world with seed {0}", seed);
            Random.InitState(seed);
            noiseOffsetX = Random.Range(int.MinValue, int.MaxValue);
            noiseOffsetY = Random.Range(int.MinValue, int.MaxValue);

            for(int x = MinBound; x < MaxBound; ++x)
            {
                flower.AddSlice(x);
            }
        }
    }
    
    private Vector3Int PickNuggetSpawnSpot()
    {
        Vector3 camPos = Camera.main.transform.position;
        int camX = (int)camPos.x;
        int lowLow = System.Math.Max(minExtent, camX - maxSpawnDistance);
        int lowHigh = System.Math.Max(lowLow, camX - minSpawnDistance);
        int highHigh = System.Math.Min(maxExtent, camX + maxSpawnDistance);
        int highLow = System.Math.Min(highHigh, camX + minSpawnDistance);
        bool canUseLow = lowLow != lowHigh;
        bool canUseHigh = highLow != highHigh;

        bool useLow = canUseLow && (!canUseHigh || Random.Range(0, 2) == 1);

        int rangeLow = useLow ? lowLow : highLow;
        int rangeHigh = useLow ? lowHigh : highHigh;

        int maxDepth = -depthLimit / 2; 

        Vector3Int chosenSpot;
        do
        {
            chosenSpot = new Vector3Int(Random.Range(rangeLow, rangeHigh), Random.Range(maxDepth, waterLevel), 0);
            maxDepth = maxDepth / 2;
        } while(GetTile(chosenSpot) != TileType.Air || !flower.isWaterPresent(chosenSpot));

        return chosenSpot;
    }

    private void Update()
    {
        Vector3 camPos = Camera.main.transform.position;
        int camX = (int)camPos.x;

        if(camX + 30 > maxExtent && maxExtent < xBounds) 
        {
            for(int x = maxExtent; x < camX + 30 && x < xBounds; ++x)
            {
                GenerateSlice(x);
            }
        }

        if(camX - 30 < minExtent && minExtent > -xBounds) 
        {
            for(int x = minExtent - 1; x > camX - 30 && x > -xBounds; --x)
            {
                GenerateSlice(x);
            }
        }

        if(Time.time - lastNuggetSpawnTime > passiveNuggetSpawnInterval)
        {
            lastNuggetSpawnTime = Time.time;

            if(passiveNuggets.Count < maxPassiveNuggets)
            {
                Vector3Int spawnPos = PickNuggetSpawnSpot();
                GameObject toSpawn = randomDebris[Random.Range(0, randomDebris.Length)];
                GameObject drop = Instantiate(toSpawn, terrainTiles.CellToWorld(spawnPos), Quaternion.identity);
                drop.GetComponent<Pickup>().OnPickup.AddListener(OnPassiveNuggetPickedUp);
                drop.transform.parent = passiveSpawns;
                //drop.GetComponent<WaterInteraction>().waterGravityScale = 0.05f;
                passiveNuggets.AddLast(drop);
            }
            else
            {
                foreach(GameObject nugget in passiveNuggets)
                {
                    if(Vector2.Distance(camPos, nugget.transform.position) > maxSpawnDistance)
                    {
                        passiveNuggets.Remove(nugget);
                        Destroy(nugget);
                        break;
                    }
                }
            }   
        }
    }

    private void OnPassiveNuggetPickedUp(GameObject nugget)
    {
        passiveNuggets.Remove(nugget);
    }

    private void GenerateSlice(int x)
    {
        float heightSamplePos =  (x - xBounds) / 40.0f;

        float hilliness = Mathf.PerlinNoise(heightSamplePos / 2, noiseOffsetY - 1.0f);
        float hill = Helpers.Remap(hilliness * Mathf.PerlinNoise(heightSamplePos, noiseOffsetY), 0, 1, -minDepth, -depthLimit / 3);
        float micro = Helpers.Remap(hilliness * Mathf.PerlinNoise(heightSamplePos * 20, noiseOffsetY), 0, 1, 0, -10);
        float height = hill + micro;
        float sandThickness = Helpers.Remap(Mathf.PerlinNoise(heightSamplePos, noiseOffsetY + 0.5f), 1, 0, sandThickAvg + sandThickVariance, sandThickAvg - sandThickVariance);

        for(int y = -depthLimit; y < hill; ++y)
        {
            Tile toPlace = y < (height - sandThickness) ? rockTile : sandTile;

            if(y != -depthLimit)
            {
                float holeSampleXpos = (x - xBounds) / 12.0f;
                float holeSampleYpos = (y) / 51.0f  + noiseOffsetY;

                float plasmaSampleXpos = (x) / 32.0f;
                float plasmaSampleYpos = (y) / 5.0f  + noiseOffsetY;
                
                float caveDensity = Helpers.Remap(y, -minDepth, -depthLimit, 0.6f, 0.85f);
                float holesCave = (Mathf.PerlinNoise(holeSampleXpos, holeSampleYpos) + Mathf.PerlinNoise(plasmaSampleXpos, plasmaSampleYpos) ) / 2.0f;
                float plasmaCave = Mathf.Sin(Mathf.PerlinNoise(plasmaSampleXpos, plasmaSampleYpos) * Mathf.PerlinNoise(plasmaSampleXpos, plasmaSampleYpos) * Mathf.PI);
                
                float caveDetail = Mathf.PerlinNoise(x / 3f, y / 3f);

                float plasmaWeight = 2;
                float holesWeight = 3;
                float noiseWeight = 1;
                
                float cave = ((holesCave * holesWeight) + (plasmaCave * plasmaWeight) + (caveDetail * noiseWeight)) / (plasmaWeight + holesWeight + noiseWeight);

                if(cave > caveDensity)
                {
                    toPlace = null;
                }

                if(toPlace == rockTile)
                {
                    float goldOreSampleXpos = (x - xBounds) / 70.0f;
                    float goldOreSampleYpos = (y) / 20.0f;
                    float goldOre = Mathf.PerlinNoise(goldOreSampleXpos, goldOreSampleYpos);
                    float goldDensity = Helpers.Remap(y, -minDepth, -depthLimit, 0.8f, 0.4f);

                    float ironOreSampleXpos = (x - xBounds) / 40.0f;
                    float ironOreSampleYpos = (y) / 15.0f;
                    float ironOre = Mathf.PerlinNoise(ironOreSampleXpos, ironOreSampleYpos);
                    float ironDensity = Helpers.Remap(y, -minDepth, -depthLimit, 0.75f, 0.6f);

                    float copperOreSampleXpos = (x - xBounds) / 55.0f;
                    float copperOreSampleYpos = (y) / 17.0f;
                    float copperOre = Mathf.PerlinNoise(copperOreSampleXpos, copperOreSampleYpos);
                    float copperDensity = Helpers.Remap(y, -minDepth, -depthLimit, 0.7f, 0.5f);

                    if(goldOre > goldDensity && Random.Range(goldDensity, 1f) < goldOre)
                    {
                        toPlace = goldTile;
                    }
                    else if(copperOre > copperDensity && Random.Range(copperDensity, 1f) < copperOre)
                    {
                        toPlace = copperTile;
                    }
                    else if(ironOre > ironDensity && Random.Range(ironDensity, 1f) < ironOre)
                    {
                        toPlace = ironTile;
                    }
                }
            }

            Vector3Int tilePos = new Vector3Int(x, y, 0);
            terrainTiles.SetTile(tilePos, toPlace);
        }

        flower.AddSlice(x);

        if(x > maxExtent) maxExtent = x;
        else if(x < minExtent) minExtent = x;
    }

    private string WriteSlice(int x)
    {
        string slice = "";
        for(int y = -depthLimit; y <= 0; ++y)
        {
            slice += GetTile(new Vector3Int(x, y, 0)) + ", ";
        }
        return slice;
    }

    public string GetSerializedTerrainForBlock()
    {
        string worldString = seed.ToString();
        for(int x = minExtent; x < maxExtent; ++x)
        {
            worldString += "\n";
            worldString += WriteSlice(x);
        }
        return worldString;
    }

    public TileBase[] GetTilesRaw()
    {
        return terrainTiles.GetTilesBlock(new BoundsInt(minExtent, -depthLimit, 0, maxExtent - minExtent, depthLimit, 1));
    }

    public void SetTilesRaw(TileBase[] tiles, int newMin, int newMax)
    {
        minExtent = newMin;
        maxExtent = newMax;
        terrainTiles.SetTilesBlock(new BoundsInt(minExtent, -depthLimit, 0, maxExtent - minExtent, depthLimit, 1), tiles);
    }

    public void SpawnItem(Pickup.ItemData itemData)
    {
        GameObject spawned = null;
        switch(itemData.pickupType)
        {
            case Pickup.Type.GoldOre:
                spawned = Instantiate(goldNuggetPrefab);
                break;
            case Pickup.Type.IronOre:
                spawned = Instantiate(ironNuggetPrefab);
                break;
            case Pickup.Type.CopperOre:
                spawned = Instantiate(copperNuggetPrefab);
                break;
        }
        if(spawned != null)
        {
            spawned.GetComponent<Pickup>().SetData(itemData);
            spawned.transform.parent = looseItems;
        } 

    }

    private GameObject SpawnDrops(TileType brokenType, Vector3Int tilePos)
    {
        GameObject toSpawn = null;
        GameObject fx = null;

        switch(brokenType)
        {
            case TileType.GoldOre:
                toSpawn = goldNuggetPrefab;
                fx = stoneBreakEffectPrefab;
                break;
            case TileType.IronOre:
                fx = stoneBreakEffectPrefab;
                toSpawn = ironNuggetPrefab;
                break;
            case TileType.CopperOre:
                fx = stoneBreakEffectPrefab;
                toSpawn = copperNuggetPrefab;
                break;
            case TileType.Stone:
                fx = stoneBreakEffectPrefab;
                break;
        }

        Vector3 position = terrainTiles.CellToWorld(tilePos) + new Vector3(0.5f, 0.5f, 0);

        if(fx != null)
        {
            position.z = 1;
            GameObject fxDrop = Instantiate(fx, position, Quaternion.identity);
        }

        if(toSpawn != null)
        {
            GameObject drop = Instantiate(toSpawn, position, Quaternion.identity);
            drop.transform.parent = looseItems;
            return drop;
        }

        return null;
    }

    
    public TileType Dig(Vector3 worldPos)
    {
        Vector3Int tilePos = terrainTiles.WorldToCell(worldPos);
        
        TileType present = GetTile(tilePos);

        if(worldPos.y <= -depthLimit) return TileType.Air;

        if(present != TileType.Air)
        {
            terrainTiles.SetTile(tilePos, null);
            flower.RegisterTerrainChange(tilePos);

            SpawnDrops(present, tilePos);
        }

        return present;
    }

    public TileType GetTile(Vector3Int tilePos)
    {
        TileBase present = terrainTiles.GetTile(tilePos);
        if(present == null) return TileType.Air;
        if(present == sandTile) return TileType.Sand;
        if(present == rockTile) return TileType.Stone;
        if(present == goldTile) return TileType.GoldOre;
        if(present == ironTile) return TileType.IronOre;
        if(present == copperTile) return TileType.CopperOre;
        else return TileType.Air;
    }

    public Vector3 SnapToTile(Vector3 worldPos)
    {
        return terrainTiles.CellToWorld(terrainTiles.WorldToCell(worldPos));
    }

    public Vector3Int SnapToGrid(Vector3 worldPos)
    {
        return terrainTiles.WorldToCell(worldPos);
    }

    public bool TilePresent(Vector3 worldPos)
    {
        return terrainTiles.GetTile(terrainTiles.WorldToCell(worldPos)) != null;
    }

    public bool IsBreathable(Vector3 worldPos)
    {
        Vector3Int tilePos = terrainTiles.WorldToCell(worldPos);
        return flower.isBreathable(tilePos);
    }
}
