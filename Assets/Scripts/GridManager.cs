using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap groundDecoratorTilemap;

    // Start is called before the first frame update
    void Start()
    {
        PrintTilemapInfo(groundTilemap);
        PrintTilemapInfo(groundDecoratorTilemap);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PrintTilemapInfo(Tilemap tilemap)
    {
        if (tilemap == null)
        {
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        print($"Tilemap Name: {tilemap.name}");
        print($"Tilemap Size: {tilemap.size.ToString()}");
        print($"Tilemap Bounds: {bounds.ToString()}");
        print($"Number of tiles: {allTiles.Length}");

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    print($"Tile: {tile.name} , Position: x: {x}, y: {y}");
                }
            }
        }
        print(" ");
    }
}
