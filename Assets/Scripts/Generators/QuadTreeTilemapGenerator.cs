using Assets.Scripts.Enums;
using Assets.Scripts.Evaluation.PathFindingEvaluation;
using Assets.Scripts.Evaluation.RoomFindingEvaluation;
using Assets.Scripts.Models;
using Assets.Scripts.Models.DataStructures;
using Assets.Scripts.Reporting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Generators
{
    class QuadTreeTilemapGenerator : TilemapGeneratorBase
    {
        public double RoomPossibility = 0.5d;
        public double SplitPossibility = 0.5d;

        private BoundsInt tilemapBounds;

        private readonly int sideSize = 100;

        private int minSplitSize;

        private QuadTree<QuadTreeLeafType> quadTree;

        private Queue<QuadTree<QuadTreeLeafType>> TreeQueue;

        private RoomFindingEvaluator evaluator;

        public void Start()
        {
            SetupTilemapGeneration();
        }


        public void SetupTilemapGeneration()
        {
            // Create tilemap if it does not exist
            tilemap = GetComponent<Tilemap>();
            if (tilemap == null)
            {
                tilemap = transform.gameObject.AddComponent<Tilemap>();
                transform.gameObject.AddComponent<TilemapRenderer>();
            }

            // Set tilemap bounds object to the value of sideSize x sideSize x 0
            tilemapBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(sideSize, sideSize, 0));


            // Create DataParser object
            fileParser = new DataParser();

            // Set minimum split size to 1/8 of the dungeon side size
            // This is a condition parameter, if the child node has a side bigger than this, the node is split into subnodes
            minSplitSize = sideSize / 8;
        }


        public override void GenerateLevel()
        {
            // Step 1. Initialize data structures for new dungeon level
            InitializeData();

            // Step 2. Create quad tree representing the dungeon
            CreateDungeonTree();

            // Step 3. Assign tile types to leafs of the Quad Tree
            AssignTileTypesToLeafs();

            // Step 4. Create tilemap from tree
            CreateTilemapFromTree();

            // Step 5. Evaluate tilemap before creating paths
            Evaluate();

            // Step 6. Create paths between rooms
            CreatePaths();

            // Step 7. Evaluate again, we expect to see only one room
            Evaluate();

            // Step 7. Write tilemap to file
            fileParser.WriteTilemap(tilemap);

        }

        public void InitializeData()
        {
            // Initialize QuadTree
            quadTree = new QuadTree<QuadTreeLeafType>(TileType.WALL, tilemapBounds, 0, RoomPossibility, new System.Random());

            // Initialize queue for splitting the tree nodes, and add the root to the queue
            TreeQueue = new Queue<QuadTree<QuadTreeLeafType>>();
            TreeQueue.Enqueue(quadTree);
        }

        public void CreateDungeonTree()
        {
            while (TreeQueue.Count > 0)
            {
                var node = TreeQueue.Dequeue();
                node.Split(minSplitSize, SplitPossibility);
                if (node.Children.Any())
                {
                    foreach (var child in node.Children)
                    {
                        TreeQueue.Enqueue(child);
                    }
                }
            }
        }

        public void AssignTileTypesToLeafs()
        {
            TreeQueue.Enqueue(quadTree);

            while (TreeQueue.Count > 0)
            {
                var node = TreeQueue.Dequeue();
                node.AssignTile();
                foreach (var child in node.Children)
                {
                    TreeQueue.Enqueue(child);
                }
            }
        }

        public void CreateTilemapFromTree()
        {
            quadTree.CreateTilemapFromLeafs(tilemap);
        }

        public void Evaluate()
        {
            evaluator = new RoomFindingEvaluator(tilemap);
            evaluator.Evaluate();
        }

        public void CreatePaths()
        {
            var rooms = evaluator.rooms;

            RoomArea currentRoom;
            RoomArea nextRoom;

            TileBase roomTile = TilemapHelper.GetTileByType(TileType.ROOM);

            for (int i=0; i<rooms.Count; i++)
            {
                currentRoom = rooms[i];
                if (i+1 < rooms.Count)
                {
                    nextRoom = rooms[i+1];
                    var path = CalculatePathBetweenRooms(currentRoom, nextRoom);
                    foreach (var position in path)
                    {
                        tilemap.SetTile(position, roomTile);
                    }
                }
            }
        }

        public List<Vector3Int> CalculatePathBetweenRooms(RoomArea roomA, RoomArea roomB)
        {
            // Find tiles with minimum distance between rooms
            TileObject tileA = roomA.tiles[0];
            TileObject tileB = roomB.tiles[0];
            int minDistance = ManhatanDistance(tileA, tileB);
            int distance;
            foreach (var tilea in roomA.tiles)
            {
                foreach(var tileb in roomB.tiles)
                {
                    distance = ManhatanDistance(tilea, tileb);
                    if (distance < minDistance)
                    {
                        tileA = tilea;
                        tileB = tileb;
                        minDistance = distance;
                    }
                }
            }

            TileObject startTile = tileA;
            TileObject endTile = tileB;
            if (tileA.Position.x > tileB.Position.x)
            {
                startTile = tileB;
                endTile = tileA;
            }

            List<Vector3Int> path = new List<Vector3Int>();
            while(startTile.Position.x != endTile.Position.x || startTile.Position.y != endTile.Position.y)
            {
                if (startTile.Position.x < endTile.Position.x)
                {
                    startTile.Position = new Vector3Int(startTile.Position.x + 1, startTile.Position.y, startTile.Position.z);
                }
                else if (startTile.Position.y < endTile.Position.y)
                {
                    startTile.Position = new Vector3Int(startTile.Position.x, startTile.Position.y + 1, startTile.Position.z);
                }
                else if (startTile.Position.y > endTile.Position.y)
                {
                    startTile.Position = new Vector3Int(startTile.Position.x, startTile.Position.y - 1, startTile.Position.z);
                }
                path.Add(startTile.Position);
            }

            return path;
            
        }

        public int ManhatanDistance(TileObject tileA, TileObject tileB)
        {
            return Mathf.Abs(tileA.Position.x - tileB.Position.x) + Mathf.Abs(tileA.Position.y - tileB.Position.y);
        }
    }
}
