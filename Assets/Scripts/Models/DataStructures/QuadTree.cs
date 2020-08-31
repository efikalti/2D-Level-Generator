using Assets.Scripts.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models.DataStructures
{
    public class QuadTree<T> : IEnumerable<QuadTree<T>>
    {

        public TileType Type { get; set; }
        public QuadTree<T> Parent { get; set; }
        public List<QuadTree<T>> Children { get; set; }

        public BoundsInt LeafBounds;

        public double RoomPossibility;

        public int Id;

        public int MinRoomSize = 5;

        protected System.Random RandomGenerator;

        public QuadTree(TileType type, BoundsInt bounds, int id, double roomPosibility, System.Random randomGenerator)
        {
            Type = type;
            Children = new List<QuadTree<T>>();
            LeafBounds = bounds;
            RoomPossibility = roomPosibility;
            Id = id;
            RandomGenerator = randomGenerator;
        }

        public QuadTree<T> AddChild(TileType child, BoundsInt bounds, int id)
        {
            QuadTree<T> childNode = new QuadTree<T>(child, bounds, id, RoomPossibility, this.RandomGenerator) { Parent = this };
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

        public void Split(int minSize, double possibility = 0)
        {
            // Check if you can split this node into 4 leafs
            if (LeafBounds.size.x <= minSize || LeafBounds.size.y <= minSize)
            {
                return;
            }

            // Check if this node is not already split
            if (Children.Count != 0)
            {
                return;
            }

            var possibilityToSplit = RandomGenerator.NextDouble();

            bool ShouldSplit = false;

            // We should split the node if any of the following conditions are true
            // 1. The size of the side of this node is larger than the minimum side size
            // 2. The random possibilityToSplit is greater than the possibility
            if ( (LeafBounds.size.x >= minSize && LeafBounds.size.y >= minSize) ||
                 (possibilityToSplit <= possibility) )
            {
                ShouldSplit = true;
            }


            if (ShouldSplit)
            {
                int id = Id + 1;
                var topLeftBounds = CalculateTopLeftBounds();
                AddChild(TileType.WALL, topLeftBounds, id++);

                var topRightBounds = CalculateTopRightBounds();
                AddChild(TileType.WALL, topRightBounds, id++);

                var bottomLeftBounds = CalculateBottomLeftBounds();
                AddChild(TileType.WALL, bottomLeftBounds, id++);

                var bottomRightBounds = CalculateBottomRightBounds();
                AddChild(TileType.WALL, bottomRightBounds, id++);
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

        public void AssignTile()
        {
            if (Children.Count == 0)
            {
                if (RandomGenerator.NextDouble() <= RoomPossibility)
                {
                    if (!IsLastNodeWithLeftAndTopWalls())
                    {
                        Type = TileType.ROOM;
                    }
                }
            }
        }

        public void CreateTilemapFromLeafs(Tilemap tilemap)
        {
            if (Children.Count == 0)
            {
                TileBase tile = TilemapHelper.GetTileByType(Type);

                TilemapHelper.FillAreaWithTile(this.LeafBounds, tile, tilemap);
            }
            else
            {
                foreach (var child in Children)
                {
                    child.CreateTilemapFromLeafs(tilemap);
                }
            }
        }

        public bool IsLastNodeWithLeftAndTopWalls()
        {
            // Check if the left and top neighbors are walls and the diagonal room
            if (Parent.Children[3].Id == Id)
            {
                if (Parent.Children[0].Type == TileType.ROOM &&
                    Parent.Children[1].Type == TileType.WALL &&
                    Parent.Children[2].Type == TileType.WALL)
                {
                    return true;
                }
            }
            return false;
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
