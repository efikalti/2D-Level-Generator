using Assets.Scripts.Controllers.TilemapController;
using Assets.Scripts.Enums;
using Assets.Scripts.Models.DataStructures;
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

        private Queue<QuadTree<QuadTreeLeafType>> SplitQueue;

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
            quadTree = new QuadTree<QuadTreeLeafType>(QuadTreeLeafType.WALL, tilemapBounds);

            // Initialize queue for splitting the tree nodes, and add the root to the queue
            SplitQueue = new Queue<QuadTree<QuadTreeLeafType>>();
            SplitQueue.Enqueue(quadTree);

            GenerateLevel();
        }


        public void GenerateLevel()
        {
            int count = 0;
            int maxCount = 50;

            double possibility = 1d;

            while(SplitQueue.Count > 0)
            {
                var node = SplitQueue.Dequeue();
                node.Split(possibility);
                foreach(var child in node.Children)
                {
                    SplitQueue.Enqueue(child);
                }
                count++;
                if (count == maxCount)
                {
                    possibility = 0.5;
                }
            }

            // Write tilemap to file
            fileParser.WriteTilemap(tilemap);

        }
    }
}
