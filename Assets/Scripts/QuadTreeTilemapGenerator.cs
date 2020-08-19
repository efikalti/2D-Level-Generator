using Assets.Scripts.Controllers.TilemapController;
using Assets.Scripts.Enums;
using Assets.Scripts.Models.DataStructures;
using Assets.Scripts.Models.Evaluation.RoomFindingEvaluation;
using Assets.Scripts.Reporting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    class QuadTreeTilemapGenerator : TilemapControllerBase
    {
        public double RoomPossibility = 0.5d;
        public double SplitPossibility = 0.5d;

        private BoundsInt tilemapBounds;

        private readonly int sideSize = 100;

        private int minSplitSize;

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

            // Step 6. Evaluate tilemap
            Evaluate();

            // Step 5. Write tilemap to file
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
            RoomFindingEvaluation evaluation = new RoomFindingEvaluation(tilemap);
            evaluation.Evaluate();
        }
    }
}
