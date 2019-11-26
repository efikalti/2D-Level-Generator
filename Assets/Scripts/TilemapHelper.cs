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
        public TileBase GetTileByType(TILE_TYPE? tileType)
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
    }
}
