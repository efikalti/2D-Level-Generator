using Assets.Scripts.Enums;
using System.Collections.Generic;

namespace Assets.Scripts.Models.DataStructures
{
    public class Node
    {
        public string Name;

        public TILE_TYPE Type;

        public Dictionary<string, Node> Links;

        public Node(string name, TILE_TYPE type) {
            Name = name;
            Type = type;
            Links = new Dictionary<string, Node>();
        }

        public void AddLink(Node node)
        {
            if (string.IsNullOrWhiteSpace(node.Name))
            {
                return;
            }

            if (Links.ContainsKey(node.Name))
            {
                return;
            }

            Links.Add(node.Name, node);
        }

        public Node GetLink(string name)
        {
            if (Links.TryGetValue(name, out Node node))
            {
                return node;
            }
            return null;
        }
    }
}
