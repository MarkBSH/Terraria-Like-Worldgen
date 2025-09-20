using System.Collections.Generic;
using UnityEngine;

public class TileRenderer : MonoBehaviour
{
    private GameObject player;
    private List<GameObject> allTiles = new();
    [SerializeField] private float maxRendererDist;
    [SerializeField] private float rendererUpdateLength;
    private float rendererUpdateTimer;

    void Awake()
    {
        player = GameObject.Find("Player(Clone)");
        GameObject[] tempAllTiles = GameObject.FindGameObjectsWithTag("Tile");
        // Initialize the allTiles list with all tiles found in the scene
        for (int i = 0; i < tempAllTiles.Length; i++)
        {
            allTiles.Add(tempAllTiles[i]);
        }
    }

    void Update()
    {
        if (rendererUpdateTimer <= rendererUpdateLength)
        {
            rendererUpdateTimer += Time.deltaTime;
        }
        else
        {
            for (int i = 0; i < allTiles.Count; i++)
            {
                // If the block is within the max distance from the player, it will be active, otherwise it will be inactive
                float dist;
                dist = Vector2.Distance(allTiles[i].transform.position, player.transform.position);
                if (dist < maxRendererDist)
                {
                    allTiles[i].SetActive(true);
                }
                else
                {
                    allTiles[i].SetActive(false);
                }
            }
            rendererUpdateTimer = 0;
        }
    }

    // For when the player can place blocks later
    public void AddBlocks(GameObject extraTile)
    {
        allTiles.Add(extraTile);
    }
}
