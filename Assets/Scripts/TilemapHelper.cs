using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public static class TilemapHelper
    {
        private static readonly System.Random randomGenerator = new System.Random();
        private static List<TileItem> _tilesArray = null;

        private static List<TileItem> TilesArray => _tilesArray ?? SetupTilesArray();

        private static List<TileItem> SetupTilesArray()
        {
            var tilePaths = Configuration.TileBasePaths;
            _tilesArray = new List<TileItem>();
            foreach(var path in tilePaths)
            {
                var tileBase = (TileBase)AssetDatabase.LoadAssetAtPath(path, typeof(TileBase));
                if (tileBase != null)
                {
                    var tileItem = new TileItem
                    {
                        Tilebase = tileBase,
                        TileType = GetTileTypeFromSpriteName(tileBase.name)
                    };
                    _tilesArray.Add(tileItem);
                }
            }
            return _tilesArray;
        }

        /// <summary>
        /// Returns a random tile from the TilesArray according
        /// The random tile is selected according to the tile type frequency
        /// </summary>
        /// <returns></returns>
        public static TileBase GetRandomTile()
        {
            int index = randomGenerator.Next(0, TilesArray.Count);

            return TilesArray[index].Tilebase;
        }

        /// <summary>
        /// Returns the TileBase that has that TILE_TYPE from the TilesArray array
        /// </summary>
        /// <param name="tileType">The TILE_TYPE of the TileBase we want to find</param>
        /// <returns></returns>
        public static TileBase GetTileByType(TileType? tileType)
        {
            foreach (var tile in TilesArray)
            {
                if (tile.TileType == tileType)
                {
                    return tile.Tilebase;
                }
            }
            return null;
        }

        /// <summary>
        /// Helper print function that prints a 3x3 tile area from top left to bottom right
        /// The tiles marked by null are not set in the neighborhood
        /// </summary>
        /// <param name="position">The center tile position</param>
        /// <param name="neighbors">The 1D array containing the 3x3 tile neighborhood</param>
        public static void PrintTileNeighborhood(Vector3Int position, TileBase[] neighbors)
        {
            int count = 0;
            string str = $"Neighborhood of tile {position.ToString()} {Environment.NewLine}";
            foreach (var tile in neighbors)
            {
                if (tile != null)
                {
                    str += $"{tile.name}    ";
                }
                else
                {
                    str += $"null    ";
                }
                count++;
                if (count % 3 == 0)
                {
                    str += Environment.NewLine;
                }
            }
            Debug.Log(str);
        }

        public static bool IsNextToType(TileType[] neighbors, TileType type)
        {
            if (neighbors == null ||
                neighbors.Length < 8)
            {
                return false;
            }

            int count = 0;
            foreach(var neighbor in neighbors)
            {
                if (count != (int)TilePositions.MIDDLE)
                {
                    if (neighbor == type)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static TileType GetTileTypeFromSpriteName(string name)
        {
            var defaultType = TileType.CORRIDOR;
            if (string.IsNullOrWhiteSpace(name))
            {
                return defaultType;
            }

            if (name.Equals(Configuration.RoomTileName))
            {
                return TileType.ROOM;
            }
            else if (name.Equals(Configuration.CorridorTileName))
            {
                return TileType.CORRIDOR;
            }
            else if (name.Equals(Configuration.WallTileName))
            {
                return TileType.WALL;
            }
            else
            {
                return defaultType;
            }
        }

        public static Vector3Int GetNeighborPosition(Vector3Int position, TilePositions neighborPosition)
        {

            return neighborPosition switch
            {
                TilePositions.DOWN_LEFT => new Vector3Int(position.x - 1, position.y - 1, 0),
                TilePositions.DOWN_MIDDLE => new Vector3Int(position.x, position.y - 1, 0),
                TilePositions.DOWN_RIGHT => new Vector3Int(position.x + 1, position.y - 1, 0),
                TilePositions.MIDDLE_LEFT => new Vector3Int(position.x - 1, position.y, 0),
                TilePositions.MIDDLE => position,
                TilePositions.MIDDLE_RIGHT => new Vector3Int(position.x + 1, position.y, 0),
                TilePositions.TOP_LEFT => new Vector3Int(position.x - 1, position.y + 1, 0),
                TilePositions.TOP_MIDDLE => new Vector3Int(position.x, position.y + 1, 0),
                TilePositions.TOP_RIGHT => new Vector3Int(position.x + 1, position.y + 1, 0),
                _ => position,
            };
        }

        /// <summary>
        /// Fill the bounded tilemap area with one type of tile
        /// </summary>
        /// <param name="bounds">The bounded tilemap area to fill with this tile</param>
        public static void FillAreaWithTile(BoundsInt bounds, TileBase tile, Tilemap tilemap)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        public static TileItem GetDefaultTile()
        {
            foreach (var tile in TilesArray)
            {
                if (tile.TileType == TileType.ROOM)
                {
                    return tile;
                }
            }
            return null;
        }

        public static bool IsTileOfType(string name, TileType type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var tileType = GetTileTypeFromSpriteName(name);

            return tileType == type;
        }
    }
}
