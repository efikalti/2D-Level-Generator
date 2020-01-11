using Assets.Scripts.Controllers.TilemapController;
using Assets.Scripts.Models;
using Assets.Scripts.Models.DataStructures;
using Assets.Scripts.Reporting;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public class TilemapLoader : TilemapControllerBase
    {
        public void Start()
        {
            SetupTilemapLoad();
        }

        public void SetupTilemapLoad()
        {
            // Create tilemap if it does not exist
            tilemap = GetComponent<Tilemap>();
            if (tilemap == null)
            {
                tilemap = transform.gameObject.AddComponent<Tilemap>();
                transform.gameObject.AddComponent<TilemapRenderer>();
            }

            // Initialize TilemapHelper object
            tilemapHelper = new TilemapHelper(TilesArray);

            // Create GraphParser object
            fileParser = new DataParser();
            fileParser.LoadInputFiles();

            // Load Tilemap object from file
            var TilemapList = fileParser.LoadNextFile();

            // Create tilemap from graph
            LoadTilemapFromList(TilemapList);

        }

        public void InitTilemaps ()
        {
            // Initialize Wall tilemap
            if (Walls == null)
            {
                Walls = new GameObject("Walls");
                Walls.transform.parent = this.transform;
                Walls.transform.position = this.transform.position;
            }
            WallTilemap = Walls.AddComponent<Tilemap>();
            Walls.AddComponent<TilemapRenderer>();


            // Initialize Floor tilemap
            if (Floors == null)
            {
                Floors = new GameObject("Floors");
                Floors.transform.parent = this.transform;
                Floors.transform.position = this.transform.position;
            }
            FloorTilemap = Floors.AddComponent<Tilemap>();
            Floors.AddComponent<TilemapRenderer>();


            // Initialize Corridor tilemap
            if (Corridors == null)
            {
                Corridors = new GameObject("Corridors");
                Corridors.transform.parent = this.transform;
                Corridors.transform.position = this.transform.position;
            }
            CorridorTilemap = Corridors.AddComponent<Tilemap>();
            Corridors.AddComponent<TilemapRenderer>();
        }

        public void LoadTilemapFromGraph(Graph graph)
        {
            foreach (var node in graph.Nodes)
            {
                tilemap.SetTile(node.Position, tilemapHelper.GetTileByType(node.Type));
            }
        }

        public void LoadTilemapFromList(List<TileObject> tiles)
        {
            foreach (var tile in tiles)
            {
                tilemap.SetTile(tile.Position, tilemapHelper.GetTileByType(tile.Type));
            }
        }

        public void LoadNextTilemapFromFile()
        {
            var tiles = fileParser.LoadNextFile();
            foreach (var tile in tiles)
            {
                tilemap.SetTile(tile.Position, tilemapHelper.GetTileByType(tile.Type));
            }
        }

        public void LoadPreviousTilemapFromFile()
        {
            var tiles = fileParser.LoadPreviousFile();
            foreach (var tile in tiles)
            {
                tilemap.SetTile(tile.Position, tilemapHelper.GetTileByType(tile.Type));
            }
        }
    }
}
