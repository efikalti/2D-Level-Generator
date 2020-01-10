using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using Assets.Scripts.Models.DataStructures;
using Assets.Scripts.Reporting;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public partial class TilemapManager
    {
        public bool GenerateNewLevel = true;

        public TileItem[] TilesArray;

        public int numberOfRooms = 5;

        private Tilemap tilemap;
        private BoundsInt tilemapBounds;

        // GameObjects for Tilemaps
        public GameObject Walls;
        public GameObject Floors;
        public GameObject Corridors;

        // TODO: Maybe remove 
        // Tilemaps
        private Tilemap WallTilemap;
        private Tilemap FloorTilemap;
        private Tilemap CorridorTilemap;

        private TilemapHelper tilemapHelper;

        private readonly Dictionary<TRANFROM_RULE, ITransformRule> rules = new Dictionary<TRANFROM_RULE, ITransformRule>();

        private readonly int sideSize = 30;
        private readonly int neighborOffset = 1;
        private readonly int neighborhoodSize = 3;

        private TileItem defaultTile;

        private System.Random randomGenerator = new System.Random();

        private Graph TilemapGraph;

        private const string defaultNodeName = "node";

        private DataParser graphParser;

        public void SetupTilemapGeneration()
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
            // Set tilemap bounds object to the value of sideSize x  sideSize x 0
            tilemapBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(sideSize, sideSize, 0));

            // Initialize and fill the rules dictionary
            rules.Add(TRANFROM_RULE.WALL_FROM_BOUNDS, new TransformToTileFromBounds(TILE_TYPE.WALL));
            rules.Add(TRANFROM_RULE.WALL_FROM_ADJACENTS, new TransformToWallFromAdjacents());
            rules.Add(TRANFROM_RULE.WALL_FOR_ROOM, new TransformToWallForRoom());
            rules.Add(TRANFROM_RULE.FLOOR_FROM_ADJACENTS, new TransformToRoomFromAdjacents());
            rules.Add(TRANFROM_RULE.FLOOR_FROM_BOUNDS, new TransformToTileFromBounds(TILE_TYPE.CORRIDOR));

            foreach (var tile in TilesArray)
            {
                if (tile.TileType == TILE_TYPE.ROOM_1)
                {
                    defaultTile = tile;
                }
            }

            // Create Graph object
            TilemapGraph = new Graph();

            // Create GraphParser object
            graphParser = new DataParser();

            GenerateLevel();
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
            graphParser = new DataParser();

            // Load Tilemap object from file
            var TilemapList = graphParser.ReadTilemap();

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
    }
}
