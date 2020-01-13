using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public class TilemapHelper
    {
        private readonly System.Random randomGenerator = new System.Random();
        private readonly TileItem[] TilesArray;

        public TilemapHelper() { }

        // TODO: Remove from constructor tilesArray
        public TilemapHelper(TileItem[] tilesArray) => TilesArray = tilesArray;

        /// <summary>
        /// Returns a random tile from the TilesArray according
        /// The random tile is selected according to the tile type frequency
        /// </summary>
        /// <returns></returns>
        public TileBase GetRandomTile()
        {
            int percentage = randomGenerator.Next(1, 100);
            foreach (TileItem tileItem in TilesArray)
            {
                if (tileItem.Percentage >= percentage)
                {
                    return tileItem.Tile;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the TileBase that has that TILE_TYPE from the TilesArray array
        /// </summary>
        /// <param name="tileType">The TILE_TYPE of the TileBase we want to find</param>
        /// <returns></returns>
        public TileBase GetTileByType(TileType? tileType)
        {
            foreach (var tile in TilesArray)
            {
                if (tile.TileType == tileType)
                {
                    return tile.Tile;
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
        public void PrintTileNeighborhood(Vector3Int position, TileBase[] neighbors)
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

        public bool IsNextToType(TileType[] neighbors, TileType type)
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

        public TileType GetTileTypeFromSpriteName(string name)
        {
            var defaultType = TileType.CORRIDOR;
            if (string.IsNullOrWhiteSpace(name))
            {
                return defaultType;
            }

            if (name.Equals(TileName.Room.Value))
            {
                return TileType.ROOM;
            }
            else if (name.Equals(TileName.Floor.Value))
            {
                return TileType.CORRIDOR;
            }
            else if (name.Equals(TileName.Wall.Value))
            {
                return TileType.WALL;
            }
            else
            {
                return defaultType;
            }
        }

        public Vector3Int GetNeighborPosition(Vector3Int position, TilePositions neighborPosition)
        {

            switch(neighborPosition)
            {
                case TilePositions.DOWN_LEFT:
                    return new Vector3Int(position.x - 1, position.y - 1, 0);
                case TilePositions.DOWN_MIDDLE:
                    return new Vector3Int(position.x, position.y - 1, 0);
                case TilePositions.DOWN_RIGHT:
                    return new Vector3Int(position.x + 1, position.y - 1, 0);
                case TilePositions.MIDDLE_LEFT:
                    return new Vector3Int(position.x - 1, position.y, 0);
                case TilePositions.MIDDLE:
                    return position;
                case TilePositions.MIDDLE_RIGHT:
                    return new Vector3Int(position.x + 1, position.y, 0);
                case TilePositions.TOP_LEFT:
                    return new Vector3Int(position.x - 1, position.y + 1, 0);
                case TilePositions.TOP_MIDDLE:
                    return new Vector3Int(position.x, position.y + 1, 0);
                case TilePositions.TOP_RIGHT:
                    return new Vector3Int(position.x + 1, position.y + 1, 0);
                default:
                    return position;
            }
        }
    }
}
