using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBasicWorld : MonoBehaviour
{
    private static CreateBasicWorld m_Instance;
    public static CreateBasicWorld Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<CreateBasicWorld>();
                if (m_Instance == null)
                {
                    GameObject _obj = new()
                    {
                        name = typeof(CreateBasicWorld).Name
                    };
                    m_Instance = _obj.AddComponent<CreateBasicWorld>();
                }
            }
            return m_Instance;
        }
    }


    [System.Serializable]
    public class WorldObjects
    {
        [Header("Tile")]
        public Sprite tileSprite;

        [Header("Space")]
        public Color cloudColor = new();

        [Header("Biomes")]
        public List<BiomesInfo> biomes;
        public List<float> biomeLocationPercent;
        public List<int> biomeLocation;

        public Color hellStoneColor = new();
    }
    public WorldObjects WorldObject = new();

    [Header("GameRules")]
    [SerializeField] GameObject player;
    [SerializeField] GameObject gameManager;

    [Header("WorldSettings")]
    public int worldLength;
    public int worldHeight;
    float unevenExtraLength;
    float unevenExtraHeight;
    [Space(5)]
    public float worldScale;
    [Space(5)]
    public float seed = 0;

    [Header("TerrainSettings")]
    [SerializeField] float heightMultiplier;
    [SerializeField] float heightMinimumPercent;
    int heightMinimum;
    [SerializeField] float underBlocksStartPercent;
    int underBlocksStart;
    [SerializeField] float underBlocksVariationPercent;
    int underBlocksVariation;
    [SerializeField] float hellBlocksStartPercent;
    int hellBlocksStart;
    [SerializeField] float hellBlocksVariationPercent;
    int hellBlocksVariation;
    [SerializeField] float cloudMinimumLayerPercent;
    int cloudMinimumLayer;
    [Space(5)]
    [Tooltip("NoiseFrequencies")]
    [SerializeField] float caveFreq;
    [SerializeField] float terrainFreq;
    public Texture2D noiseMap;

    [Header("InvisOptions")]
    int whatBiome;

    void Awake()
    {
        // Random seed generation
        seed = Random.Range(-1000000, 1000000);

        // Height and width calculation from percentages to actual values
        heightMinimum = Mathf.RoundToInt(heightMinimumPercent * (worldHeight / 100));
        underBlocksStart = Mathf.RoundToInt(underBlocksStartPercent * (worldHeight / 100) * (1 / worldScale));
        underBlocksVariation = Mathf.RoundToInt(underBlocksVariationPercent * (worldHeight / 100) * (1 / worldScale));
        hellBlocksStart = Mathf.RoundToInt(hellBlocksStartPercent * (worldHeight / 100) * (1 / worldScale));
        hellBlocksVariation = Mathf.RoundToInt(hellBlocksVariationPercent * (worldHeight / 100) * (1 / worldScale));
        cloudMinimumLayer = Mathf.RoundToInt(cloudMinimumLayerPercent * (worldHeight / 100));
        for (int i = 0; i < WorldObject.biomeLocationPercent.Count; i++)
        {
            WorldObject.biomeLocation.Add(Mathf.RoundToInt(WorldObject.biomeLocationPercent[i] * (worldLength / 100) * (1 / worldScale)));
        }
        for (int i = 0; i < WorldObject.biomes.Count; i++)
        {
            WorldObject.biomes[i].biomeLength = Mathf.RoundToInt(WorldObject.biomes[i].biomeLengthPercent * (worldLength / 100));
            WorldObject.biomes[i].biomeLengthVariation = Mathf.RoundToInt(WorldObject.biomes[i].biomeLengthVariationPercent * (worldLength / 100));
            WorldObject.biomes[i].biomeDepth = Mathf.RoundToInt(WorldObject.biomes[i].biomeDepthPercent * (worldHeight / 100));
            WorldObject.biomes[i].biomeDepthVariation = Mathf.RoundToInt(WorldObject.biomes[i].biomeDepthVariationPercent * (worldHeight / 100));
        }
    }

    void Start()
    {
        MakeBasicWorld();
    }

    private void MakeBasicWorld()
    {
        GenerateNoiseMap();
        StartCoroutine(MakeBasicTerrain());
        StartCoroutine(MakeBasicClouds());
    }

    // Generates a noise map for cave generation
    void GenerateNoiseMap()
    {
        noiseMap = new Texture2D(Mathf.RoundToInt(worldLength * (1 / worldScale)), Mathf.RoundToInt(worldHeight * (1 / worldScale)));

        for (int x = 0; x < noiseMap.width; x++)
        {
            for (int y = 0; y < noiseMap.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                noiseMap.SetPixel(x, y, new Color(v, v, v));
            }
        }
        noiseMap.Apply();
    }

    private IEnumerator MakeBasicTerrain()
    {
        for (int x = 0; x < worldLength * (1 / worldScale); x++)
        {
            // Generate a noise map for terrain height
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + (heightMinimum * (1 / worldScale));
            for (int y = 0; y < height; y++)
            {
                if (noiseMap.GetPixel(x, y).r > 0.22 || y >= height - 4)
                {
                    // Check for what biome this tile is in
                    CheckBiome(x, y);
                    // Check for if the tile is the highest tile
                    if (y >= height - 1)
                    {
                        SpawnTopTile(x, y);
                    }
                    else
                    {
                        // If the tile is not the highest, spawn the appropriate tile based on the biome and height
                        float dirtOrStone = y + Random.Range(-underBlocksVariation, underBlocksVariation);
                        if (dirtOrStone > underBlocksStart)
                        {
                            SpawnAboveTile(x, y);
                        }
                        else
                        {
                            // If the tile is below the under blocks start, check if it should be hell stone or regular stone
                            float stoneOrHellStone = y + Random.Range(-hellBlocksVariation, hellBlocksVariation);
                            if (stoneOrHellStone > hellBlocksStart)
                            {
                                SpawnUnderTile(x, y);
                            }
                            else
                            {
                                SpawnHellStoneTile(x, y);
                            }
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.01f);
        }

        // After generating the terrain, load the last game objects
        LastGameObjectsLoader();
    }

    private IEnumerator MakeBasicClouds()
    {
        // Generate a noise map for clouds
        for (int x = 0; x < worldLength * (1 / worldScale); x++)
        {
            for (int y = Mathf.RoundToInt(cloudMinimumLayer * (1 / worldScale)); y < worldHeight * (1 / worldScale); y++)
            {
                if (noiseMap.GetPixel(x, y).r < 0.22)
                {
                    SpawnCloudTile(x, y);
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    private void LastGameObjectsLoader()
    {
        Instantiate(player, transform.position, transform.rotation);
        Instantiate(gameManager, transform.position, transform.rotation);
    }

    private void SpawnTopTile(int x, int y)
    {
        SpawnAllTiles(WorldObject.biomes[whatBiome].topTileName, WorldObject.biomes[whatBiome].topTileColor, x, y);
    }
    private void SpawnAboveTile(int x, int y)
    {
        SpawnAllTiles(WorldObject.biomes[whatBiome].aboveTileName, WorldObject.biomes[whatBiome].aboveTileColor, x, y);
    }
    private void SpawnUnderTile(int x, int y)
    {
        SpawnAllTiles(WorldObject.biomes[whatBiome].underTileName, WorldObject.biomes[whatBiome].underTileColor, x, y);
    }
    private void SpawnHellStoneTile(int x, int y)
    {
        SpawnAllTiles("HellStoneTile", WorldObject.hellStoneColor, x, y);
    }
    private void SpawnCloudTile(int x, int y)
    {
        SpawnAllTiles("CloudTile", WorldObject.cloudColor, x, y);
    }

    private void SpawnAllTiles(string tileName, Color color, int x, int y)
    {
        // Makes a new tile GameObject with the specified name and color
        GameObject newTile = new GameObject(name = tileName)
        {
            tag = "Tile"
        };
        newTile.transform.parent = transform;
        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = WorldObject.tileSprite;
        newTile.GetComponent<SpriteRenderer>().color = color;

        newTile.transform.position = new Vector3(-worldLength / 2 + x * worldScale + unevenExtraLength, -worldHeight / 2 + y * worldScale + unevenExtraHeight);
        newTile.transform.localScale = new Vector2(worldScale, worldScale);
    }

    void CheckBiome(float x, float y)
    {
        // Check in what biome the tile is located
        for (int i = 0; i < WorldObject.biomes.Count; i++)
        {
            int widthPoint = WorldObject.biomeLocation[i];

            int depthPoint = Mathf.RoundToInt(heightMultiplier + (heightMinimum * (1 / worldScale))) - 20;


            int lengthCheker = Mathf.RoundToInt(x + Random.Range(-WorldObject.biomes[i].biomeLengthVariation, WorldObject.biomes[i].biomeLengthVariation) * (1 / worldScale));

            int heightCheker = Mathf.RoundToInt(y + Random.Range(-WorldObject.biomes[i].biomeDepthVariation, WorldObject.biomes[i].biomeDepthVariation) * (1 / worldScale));

            if (lengthCheker > widthPoint + -WorldObject.biomes[i].biomeLength && lengthCheker < widthPoint + WorldObject.biomes[i].biomeLength)
            {
                if (heightCheker > depthPoint + -WorldObject.biomes[i].biomeDepth && heightCheker < depthPoint + WorldObject.biomes[i].biomeDepth)
                {
                    whatBiome = i;
                }
            }
        }
    }
}
