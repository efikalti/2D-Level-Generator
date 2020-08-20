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
            foreach (var path in tilePaths)
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

        public static bool IsNextToType(TileType[] neighbors, TileType type)
        {
            if (neighbors == null ||
                neighbors.Length < 8)
            {
                return false;
            }

            int count = 0;
            foreach (var neighbor in neighbors)
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

        public static TileType GetTileTypeFromPosition(Vector3Int position, Tilemap tilemap)
        {

            if (tilemap == null) return TileType.STAIRS;

            var currentTileBase = tilemap.GetTile(position);

            if (currentTileBase == null) return TileType.STAIRS;

            return GetTileTypeFromSpriteName(currentTileBase.name);

        }
    }
}
