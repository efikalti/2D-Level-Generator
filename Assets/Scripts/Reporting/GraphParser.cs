using Assets.Scripts.Enums;
using Assets.Scripts.Models.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Reporting
{
    class GraphParser
    {
        private const string BasePath = "./Assets/Data/Graphs/";
        private const string BaseFilename = "graph_";
        private const string BaseFilenameSuffix = ".csv";

        public void WriteGraph(Graph graph)
        {
            // Create filename for this csv file
            var path = BasePath 
                       + BaseFilename
                       + string.Format( "{0:yyyy_MM_ddTHH_mm_ss}", DateTime.Now)
                       + BaseFilenameSuffix;

            //path = "./Assets/Data/Graphs/graph.csv";

            string line;

            // Write graph content into file
            using (StreamWriter streamWriter = File.CreateText(path))
            {
                // Write column names
                line = string.Format("{0}, {1}, {2},", "Node ID", "Node Position", "Node Type");
                // Get neighbor tiles position names
                line += GetTilePositionsHeader();
                streamWriter.WriteLine(line);
                streamWriter.Flush();

                foreach (var node in graph.Nodes)
                {
                    line = string.Format("{0}, {1}, {2},", node.Name, node.Position.ToString(), node.Type.ToString());
                    line += WriteNodeLinks(node);
                    streamWriter.WriteLine(line);
                    streamWriter.Flush();
                }
            }
        }

        public string WriteNodeLinks(Node node)
        {
            string line = string.Empty;
            Node neighbor;
            foreach(TILE_POSITIONS position in Enum.GetValues(typeof(TILE_POSITIONS)))
            {
                if (position != TILE_POSITIONS.MIDDLE)
                {
                    neighbor = node.GetLink(position);
                    if (neighbor == null)
                    {
                        line += string.Format(" {0}", -1);
                    }
                    else
                    {
                        line += string.Format(" {0}", (int) neighbor.Type);
                    }

                    if (position != TILE_POSITIONS.TOP_RIGHT)
                    {
                        line += ",";
                    }
                }
            }
            return line;
        }

        public string GetTilePositionsHeader()
        {
            string line = string.Empty;
            foreach (TILE_POSITIONS position in Enum.GetValues(typeof(TILE_POSITIONS)))
            {
                if (position != TILE_POSITIONS.MIDDLE)
                {
                    line += string.Format("{0}", position.ToString());

                    if (position != TILE_POSITIONS.TOP_RIGHT)
                    {
                        line += ",";
                    }
                }
            }
            return line;
        }
    }
}
