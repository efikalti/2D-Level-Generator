using Assets.Scripts.Controllers.TilemapController;
using Assets.Scripts.Enums;
using Assets.Scripts.Models.DataStructures;
using Assets.Scripts.Reporting;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    class QuadTreeTilemapGenerator : TilemapControllerBase
    {
        private BoundsInt tilemapBounds;

        private readonly int sideSize = 30;

        private QuadTree<QuadTreeLeafType> quadTree;

        private Queue<QuadTree<QuadTreeLeafType>> TreeQueue;

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

            // Initialize TilemapHelper object
            tilemapHelper = new TilemapHelper(TilesArray);
            // Set tilemap bounds object to the value of sideSize x  sideSize x 0
            tilemapBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(sideSize, sideSize, 0));

            // Initialize QuadTree
            quadTree = new QuadTree<QuadTreeLeafType>(QuadTreeLeafType.WALL, tilemapBounds, 0);

            // Initialize queue for splitting the tree nodes, and add the root to the queue
            TreeQueue = new Queue<QuadTree<QuadTreeLeafType>>();
            TreeQueue.Enqueue(quadTree);

            // Create DataParser object
            fileParser = new DataParser();

            GenerateLevel();
        }


        public override void GenerateLevel()
        {
            // Step 1. Create quad tree representing the dungeon
            CreateDungeonTree();

            // Step 2. Assign tile types to leafs of the Quad Tree
            AssignTileTypesToLeafs();

            // Step 3. Create tilemap from tree
            CreateTilemapFromTree();

            // Step 4. Write tilemap to file
            fileParser.WriteTilemap(tilemap);

        }

        public void CreateDungeonTree()
        {
            int count = 0;
            int maxCount = 10;

            double possibility = 1d;

            while (TreeQueue.Count > 0)
            {
                var node = TreeQueue.Dequeue();
                node.Split(possibility);
                foreach (var child in node.Children)
                {
                    TreeQueue.Enqueue(child);
                }
                count++;
                if (count == maxCount)
                {
                    possibility = 0.5;
                }
            }
        }

        public void AssignTileTypesToLeafs()
        {

            var randomGenerator = new System.Random();

            TreeQueue.Enqueue(quadTree);

            while (TreeQueue.Count > 0)
            {
                var node = TreeQueue.Dequeue();
                node.AssignTile(randomGenerator.NextDouble());
                foreach (var child in node.Children)
                {
                    TreeQueue.Enqueue(child);
                }
            }
        }

        public void CreateTilemapFromTree()
        {
            quadTree.CreateTilemapFromLeafs(tilemap, tilemapHelper);
        }
    }
}
