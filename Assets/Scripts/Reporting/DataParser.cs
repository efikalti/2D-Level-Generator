using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Reporting
{
    public class DataParser
    {
        private const string BaseOutputPath = "Data/GAN_Input/";
        private const string BaseInputPath = "Data/GAN_Output/";
        private const string ResultsFolder = "/Results/";
        private const string BaseFilename = "graph_";
        private const string BaseFilenameSuffix = ".csv";
        private const char CSVSeparator = ',';
        private const string NullTile = "-1";
        private int CurrentFileIndex = -1;

        private string[] files;

        public DataParser()
        {
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
            if (!int.TryParse(lineParts[0], out int x)
                || !int.TryParse(lineParts[1], out int y)
                || !int.TryParse(lineParts[2], out int type))
            {
                return null;
            }


            return new TileObject(
                position: new Vector3Int(x, y, 0),
                type: (TileType)type);
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
            StreamWriter streamWriter = File.CreateText(path);
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
                        typeId = (int)TilemapHelper.GetTileTypeFromSpriteName(tile.name);
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

            streamWriter.Close();
            streamWriter.Dispose();
        }

        public void LoadInputFiles(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return;
            }

            Debug.Log($"Loading folder: {folder}");
            var inputPath = BaseInputPath + folder + ResultsFolder;

            if (Directory.Exists(inputPath))
            {
                // Get all csv files in that folder
                files = Directory.GetFiles(inputPath, "*.csv");
            }

        }

        public List<string> GetAllFoldersInBasePath()
        {
            return Directory.GetDirectories(BaseInputPath).Select(x => x.Remove(0, BaseInputPath.Length)).ToList();
        }

        public List<TileObject> LoadTilesFromFile(string filename)
        {
            Debug.Log($"Loading: {filename}");
            // Create new List
            List<TileObject> tiles = new List<TileObject>();

            if (string.IsNullOrWhiteSpace(filename))
            {
                return tiles;
            }

            TileObject tile;
            using (var rd = new StreamReader(filename))
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

        public List<TileObject> LoadNextFile()
        {
            if (files.Length == 0)
            {
                return new List<TileObject>();
            }

            CurrentFileIndex++;
            if (CurrentFileIndex >= files.Length)
            {
                CurrentFileIndex = 0;
            }
            // Get current file from list
            string file = files[CurrentFileIndex];

            return LoadTilesFromFile(file);
        }

        public List<TileObject> LoadPreviousFile()
        {
            CurrentFileIndex--;
            if (CurrentFileIndex < 0)
            {
                CurrentFileIndex = files.Length - 1;
            }

            // Get current file from list
            string file = files[CurrentFileIndex];

            return LoadTilesFromFile(file);
        }
    }
}
