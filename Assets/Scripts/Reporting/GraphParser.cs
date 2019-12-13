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
            using (StreamWriter w = File.CreateText(path))
            {
                // Write column names
                line = string.Format("{0},{1}", "Node ID", "Node Type");
                w.WriteLine(line);
                w.Flush();

                foreach(var node in graph.Nodes)
                {
                    line = string.Format("{0},{1}", node.Name, node.Type.ToString());
                    w.WriteLine(line);
                    w.Flush();
                }
            }
        }
    }
}
