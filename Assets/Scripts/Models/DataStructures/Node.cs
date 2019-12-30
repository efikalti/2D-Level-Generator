using Assets.Scripts.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Models.DataStructures
{
    public class Node : TileObject
    {
        public string Name;

        public TILE_POSITIONS TilePosition;

        public Dictionary<TILE_POSITIONS, Node> Links;

        public Node(string name, Vector3Int position, TILE_POSITIONS tilePosition, TILE_TYPE type) {
            Name = name;
            Position = position;
            TilePosition = tilePosition;
            Type = type;
            Links = new Dictionary<TILE_POSITIONS, Node>();
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

        public Node GetLink(TILE_POSITIONS tilePosition)
        {
            if (Links.TryGetValue(tilePosition, out Node node))
            {
                return node;
            }
            return null;
        }
    }
}
