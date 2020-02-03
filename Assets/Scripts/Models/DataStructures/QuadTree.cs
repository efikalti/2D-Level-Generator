using Assets.Scripts.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models.DataStructures
{
    class QuadTree<T> : IEnumerable<QuadTree<T>>
    {

        public QuadTreeLeafType Type { get; set; }
        public QuadTree<T> Parent { get; set; }
        public ICollection<QuadTree<T>> Children { get; set; }

        public BoundsInt LeafBounds;

        public double RoomPosibility;

        public int Id;

        public QuadTree(QuadTreeLeafType type, BoundsInt bounds, int id, double roomPosibility = 0.5)
        {
            Type = type;
            Children = new List<QuadTree<T>>();
            LeafBounds = bounds;
            RoomPosibility = roomPosibility;
            Id = id;
        }

        public QuadTree<T> AddChild(QuadTreeLeafType child, BoundsInt bounds, int id)
        {
            QuadTree<T> childNode = new QuadTree<T>(child, bounds, id) { Parent = this };
            Children.Add(childNode);
            return childNode;
        }

        public IEnumerator<QuadTree<T>> GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
            foreach (var directChild in Children)
            {
                foreach (var anyChild in directChild)
                    yield return anyChild;
            }
        }

        public void Split(double possibility = 1)
        {
            // Check if you can split this node into 4 leafs
            if (LeafBounds.size.x == 1 || LeafBounds.size.y == 1)
            {
                return;
            }

            // Check if this node is not already split
            if (Children.Count != 0)
            {
                return;
            }

            var randomGenerator = new System.Random();
            var possibilityToSplit = randomGenerator.NextDouble();

            if (possibilityToSplit < possibility)
            {
                int id = Id + 1;
                var topLeftBounds = CalculateTopLeftBounds();
                AddChild(QuadTreeLeafType.WALL, topLeftBounds, id++);

                var topRightBounds = CalculateTopRightBounds();
                AddChild(QuadTreeLeafType.WALL, topRightBounds, id++);

                var bottomLeftBounds = CalculateBottomLeftBounds();
                AddChild(QuadTreeLeafType.WALL, bottomLeftBounds, id++);

                var bottomRightBounds = CalculateBottomRightBounds();
                AddChild(QuadTreeLeafType.WALL, bottomRightBounds, id++);
            }
        }

        public BoundsInt CalculateTopLeftBounds()
        {
            BoundsInt bounds = new BoundsInt();
            {
                bounds.xMin = LeafBounds.xMin;
                bounds.xMax = (LeafBounds.xMax + LeafBounds.xMin) / 2;
                bounds.yMin = ((LeafBounds.yMax + LeafBounds.yMin) / 2);
                bounds.yMax = LeafBounds.yMax;
            }
            return bounds;
        }

        public BoundsInt CalculateTopRightBounds()
        {
            BoundsInt bounds = new BoundsInt();
            {
                bounds.xMin = ((LeafBounds.xMax + LeafBounds.xMin) / 2);
                bounds.xMax = LeafBounds.xMax;
                bounds.yMin = ((LeafBounds.yMax + LeafBounds.yMin) / 2);
                bounds.yMax = LeafBounds.yMax;
            }
            return bounds;
        }

        public BoundsInt CalculateBottomLeftBounds()
        {
            BoundsInt bounds = new BoundsInt();
            {
                bounds.xMin = LeafBounds.xMin;
                bounds.xMax = (LeafBounds.xMax + LeafBounds.xMin) / 2;
                bounds.yMin = LeafBounds.yMin;
                bounds.yMax = (LeafBounds.yMax + LeafBounds.yMin) / 2;
            }
            return bounds;
        }

        public BoundsInt CalculateBottomRightBounds()
        {
            BoundsInt bounds = new BoundsInt();
            {
                bounds.xMin = ((LeafBounds.xMax + LeafBounds.xMin) / 2);
                bounds.xMax = LeafBounds.xMax;
                bounds.yMin = LeafBounds.yMin;
                bounds.yMax = (LeafBounds.yMax + LeafBounds.yMin) / 2;
            }
            return bounds;
        }

        public void AssignTile(double possibility)
        {
            if (Children.Count == 0)
            {
                if (possibility <= RoomPosibility)
                {
                    Type = QuadTreeLeafType.ROOM;
                }
            }
        }

        public void CreateTilemapFromLeafs(Tilemap tilemap, TilemapHelper tilemapHelper)
        {
            if (Children.Count == 0)
            {
                TileBase tile = tilemapHelper.GetTileBaseFromLeafType(Type);

                tilemapHelper.FillAreaWithTile(this.LeafBounds, tile, tilemap);
            }
            else
            {
                foreach (var child in Children)
                {
                    child.CreateTilemapFromLeafs(tilemap, tilemapHelper);
                }
            }
        }

        public void Print()
        {
            Debug.Log($"Node {Id}, number of children: {Children.Count}, bounds: {LeafBounds.ToString()}, type: {Type}");
            if (Children.Count > 0)
            {
                foreach (var child in Children)
                {
                    child.Print();
                }
            }
        }
    }
}
