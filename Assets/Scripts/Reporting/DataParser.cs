﻿using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using Assets.Scripts.Models.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Reporting
{
    class DataParser
    {
        private const string BaseOutputPath = "./Assets/Data/Output/";
        private const string BaseInputPath = "./Assets/Data/Input/";
        private const string BaseFilename = "graph_";
        private const string BaseFilenameSuffix = ".csv";
        private const char CSVSeparator = ',';
        private const string NullTile = "-1";

        private Lazy<TilemapHelper> TilemapHelper = new Lazy<TilemapHelper>();

        public void WriteGraph(Graph graph)
        {
            // Create filename for this csv file
            var path = BaseOutputPath
                       + BaseFilename
                       + string.Format("{0:yyyy_MM_ddTHH_mm_ss}", DateTime.Now)
                       + BaseFilenameSuffix;

            string line;

            // Write graph content into file
            using (StreamWriter streamWriter = File.CreateText(path))
            {
                // Write column names
                line = string.Format("{0},{1},{2},{3},", "Node ID", "Node x", "Node y", "Node Type");
                // Get neighbor tiles position names
                line += GetTilePositionsHeader();
                streamWriter.WriteLine(line);
                streamWriter.Flush();

                foreach (var node in graph.Nodes)
                {
                    line = string.Format("{0},{1},{2},{3},", node.Name, node.Position.x, node.Position.y, (int)node.Type);
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
            foreach (TILE_POSITIONS position in Enum.GetValues(typeof(TILE_POSITIONS)))
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
                        line += string.Format(" {0}", (int)neighbor.Type);
                    }

                    if (position != TILE_POSITIONS.TOP_RIGHT)
                    {
                        line += ", ";
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

        public Graph ReadGraph()
        {
            // Create new Graph
            Graph graph = new Graph();

            // Get all files in input directory
            var files = Directory.GetFiles(BaseInputPath, "*.csv");

            if (files.Length < 1)
            {
                return graph;
            }

            // Get first file of directory
            string file = files[0];

            if (string.IsNullOrWhiteSpace(file))
            {
                return graph;
            }

            Node node;
            using (var rd = new StreamReader(file))
            {
                while (!rd.EndOfStream)
                {
                    node = ParseCSVLineToNode(rd.ReadLine());
                    if (node != null)
                    {
                        graph.AddNode(node);
                    }
                }
            }

            return graph;
        }

        public Node ParseCSVLineToNode(string line)
        {
            var lineParts = line.Split(CSVSeparator);

            // Check that there are at least 4 items
            if (lineParts.Length < 4)
            {
                return null;
            }

            // Try parsing x,y coords and tile type
            if (!int.TryParse(lineParts[1], out int x)
                || !int.TryParse(lineParts[2], out int y)
                || !int.TryParse(lineParts[3], out int type))
            {
                return null;
            }


            return new Node(
                name: lineParts[0],
                position: new Vector3Int(x, y, 0),
                tilePosition: TILE_POSITIONS.MIDDLE,
                type: (TILE_TYPE)type);
        }

        public TileObject ParseCSVLineToTile(string line)
        {
            var lineParts = line.Split(CSVSeparator);

            // Check that there are at least 3 items
            if (lineParts.Length < 3)
            {
                return null;
            }

            // Try parsing x,y coords and tile type
            if (!int.TryParse(lineParts[1], out int x)
                || !int.TryParse(lineParts[2], out int y)
                || !int.TryParse(lineParts[3], out int type))
            {
                return null;
            }


            return new TileObject(
                position: new Vector3Int(x, y, 0),
                type: (TILE_TYPE)type);
        }

        public void WriteTilemap(Tilemap tilemap)
        {
            var bounds = tilemap.cellBounds;

            // Create filename for this csv file
            var path = BaseOutputPath
                       + BaseFilename
                       + string.Format("{0:yyyy_MM_ddTHH_mm_ss}", DateTime.Now)
                       + $"_{bounds.xMax}x{bounds.yMax}"
                       + BaseFilenameSuffix;

            string line;

            // Write tilemap content into file
            using (StreamWriter streamWriter = File.CreateText(path))
            {
                // Write tilemap dimensions 
                line = string.Format("{0}, {1}", bounds.xMax - bounds.xMin, bounds.yMax - bounds.yMin);
                streamWriter.WriteLine(line);
                streamWriter.Flush();

                // Write column names
                line = string.Format("{0},{1},{2}", "Tile X", "Tile Y", "Tile Type");
                streamWriter.WriteLine(line);
                streamWriter.Flush();

                int typeId;
                TileBase tile;
                Vector3Int position = Vector3Int.zero;
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        position.x = x;
                        position.y = y;
                        tile = tilemap.GetTile(position);
                        if (tile != null)
                        {
                            typeId = (int)TilemapHelper.Value.GetTileTypeFromSpriteName(tile.name);
                            line = string.Format("{0},{1},{2}", x, y, typeId);
                        }
                        else
                        {
                            line = string.Format("{0},{1},{2}", x, y, NullTile);
                        }
                        streamWriter.WriteLine(line);
                        streamWriter.Flush();
                    }
                }

            }
        }

        public List<TileObject> ReadTilemap()
        {
            // Create new Graph
            List<TileObject> tiles = new List<TileObject>();

            // Get all files in input directory
            var files = Directory.GetFiles(BaseInputPath, "*.csv");

            if (files.Length < 1)
            {
                return tiles;
            }

            // Get first file of directory
            string file = files[0];

            if (string.IsNullOrWhiteSpace(file))
            {
                return tiles;
            }

            TileObject tile;
            using (var rd = new StreamReader(file))
            {
                while (!rd.EndOfStream)
                {
                    tile = ParseCSVLineToTile(rd.ReadLine());
                    if (tile != null)
                    {
                        tiles.Add(tile);
                    }
                }
            }

            return tiles;
        }
    }
}