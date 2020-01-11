using Assets.Scripts.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Models.DataStructures
{
    class QuadTree<T> : IEnumerable<QuadTree<T>>
    {

        public QuadTreeLeafType Type { get; set; }
        public QuadTree<T> Parent { get; set; }
        public ICollection<QuadTree<T>> Children { get; set; }

        public BoundsInt LeafBounds;

        public QuadTree(QuadTreeLeafType type, BoundsInt bounds)
        {
            Type = type;
            Children = new List<QuadTree<T>>();
            LeafBounds = bounds;
        }

        public QuadTree<T> AddChild(QuadTreeLeafType child, BoundsInt bounds)
        {
            QuadTree<T> childNode = new QuadTree<T>(child, bounds) { Parent = this };
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

                var topLeftBounds = CalculateTopLeftBounds();
                AddChild(QuadTreeLeafType.WALL, topLeftBounds);

                var topRightBounds = CalculateTopRightBounds();
                AddChild(QuadTreeLeafType.WALL, topRightBounds);

                var bottomLeftBounds = CalculateBottomLeftBounds();
                AddChild(QuadTreeLeafType.WALL, bottomLeftBounds);

                var bottomRightBounds = CalculateBottomRightBounds();
                AddChild(QuadTreeLeafType.WALL, bottomRightBounds);
            }
        }

        public BoundsInt CalculateTopLeftBounds()
        {
            BoundsInt bounds = new BoundsInt();
            {
                bounds.xMin = LeafBounds.xMin;
                bounds.xMax = (LeafBounds.xMax - LeafBounds.xMin) / 2;
                bounds.yMin = 1 + ((LeafBounds.yMax - LeafBounds.yMin) / 2);
                bounds.yMax = LeafBounds.yMax;
            }
            return bounds;
        }

        public BoundsInt CalculateTopRightBounds()
        {
            BoundsInt bounds = new BoundsInt();
            {
                bounds.xMin = 1 + ((LeafBounds.xMax - LeafBounds.xMin) / 2);
                bounds.xMax = LeafBounds.xMax;
                bounds.yMin = 1 + ((LeafBounds.yMax - LeafBounds.yMin) / 2);
                bounds.yMax = LeafBounds.yMax;
            }
            return bounds;
        }

        public BoundsInt CalculateBottomLeftBounds()
        {
            BoundsInt bounds = new BoundsInt();
            {
                bounds.xMin = LeafBounds.xMin;
                bounds.xMax = (LeafBounds.xMax - LeafBounds.xMin) / 2;
                bounds.yMin = LeafBounds.yMin;
                bounds.yMax = (LeafBounds.yMax - LeafBounds.yMin) / 2;
            }
            return bounds;
        }

        public BoundsInt CalculateBottomRightBounds()
        {
            BoundsInt bounds = new BoundsInt();
            {
                bounds.xMin = 1 + ((LeafBounds.xMax - LeafBounds.xMin) / 2);
                bounds.xMax = LeafBounds.xMax;
                bounds.yMin = LeafBounds.yMin;
                bounds.yMax = (LeafBounds.yMax - LeafBounds.yMin) / 2;
            }
            return bounds;
        }
    }
}
