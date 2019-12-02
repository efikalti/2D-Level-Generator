using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models.DataStructures
{
    public class Graph
    {

        private List<Node> Nodes { get; set; }

        public Graph() {
            Nodes = new List<Node>();
        }

        public void AddNode(Node node)
        {
            if (node != null)
            {
                Nodes.Add(node);
            }
        }
    }
}
