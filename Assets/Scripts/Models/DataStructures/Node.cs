using Assets.Scripts.Enums;
using System.Collections.Generic;

namespace Assets.Scripts.Models.DataStructures
{
    public class Node
    {
        public string Name;

        public TILE_TYPE Type;

        public TILE_POSITIONS Position;

        public Dictionary<TILE_POSITIONS, Node> Links;

        public Node(string name, TILE_POSITIONS position, TILE_TYPE type) {
            Name = name;
            Position = position;
            Type = type;
            Links = new Dictionary<TILE_POSITIONS, Node>();
        }

        public void AddLink(Node node)
        {
            if (string.IsNullOrWhiteSpace(node.Name))
            {
                return;
            }

            if (Links.ContainsKey(node.Position))
            {
                return;
            }

            Links.Add(node.Position, node);
        }

        public Node GetLink(TILE_POSITIONS position)
        {
            if (Links.TryGetValue(position, out Node node))
            {
                return node;
            }
            return null;
        }
    }
}
