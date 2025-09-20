using UnityEngine;

[CreateAssetMenu]
public class BiomesInfo : ScriptableObject
{
    [Header("BiomeName")]
    public string biomeName;

    [Header("Tiles")]
    [Tooltip("Names")]
    public string topTileName;
    public string aboveTileName;
    public string underTileName;
    [Tooltip("Colors")]
    public Color topTileColor;
    public Color aboveTileColor;
    public Color underTileColor;
    [Tooltip("Biome Size")]
    public float biomeLengthPercent;
    public int biomeLength;
    public float biomeLengthVariationPercent;
    public int biomeLengthVariation;
    public float biomeDepthPercent;
    public int biomeDepth;
    public float biomeDepthVariationPercent;
    public int biomeDepthVariation;

    void Awake()
    {
        biomeLength = Mathf.RoundToInt(biomeLengthPercent * CreateBasicWorld.Instance.worldLength / 100);
        biomeLengthVariation = Mathf.RoundToInt(biomeLengthVariationPercent * CreateBasicWorld.Instance.worldLength / 100);
        biomeDepth = Mathf.RoundToInt(biomeDepthPercent * CreateBasicWorld.Instance.worldHeight / 100);
        biomeDepthVariation = Mathf.RoundToInt(biomeDepthPercent * CreateBasicWorld.Instance.worldHeight / 100);
    }
}
