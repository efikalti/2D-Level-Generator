using Assets.Scripts.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Models.DataStructures
{
    public class Node : TileObject
    {
        public string Name;

        public TilePositions TilePosition;

        public Dictionary<TilePositions, Node> Links;

        public Node(string name, Vector3Int position, TilePositions tilePosition, TileType type) {
            Name = name;
            Position = position;
            TilePosition = tilePosition;
            Type = type;
            Links = new Dictionary<TilePositions, Node>();
        }

        public void AddLink(Node node)
        {
            if (string.IsNullOrWhiteSpace(node.Name))
            {
                return;
            }

            if (Links.ContainsKey(node.TilePosition))
            {
                return;
            }

            Links.Add(node.TilePosition, node);
        }

        public Node GetLink(TilePositions tilePosition)
        {
            if (Links.TryGetValue(tilePosition, out Node node))
            {
                return node;
            }
            return null;
        }
    }
}
