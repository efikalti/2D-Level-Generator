using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public static class TilemapTransformHelper
    {

        /// <summary>
        /// Transform the bounds area in the tilemap to a specific tile
        /// </summary>
        /// <param name="bounds">The bounded area of the tilemap to be transformed</param>
        /// <param name="tile">The tile to be applied to all bounds</param>
        public static void TransformBounds(Tilemap tilemap, BoundsInt bounds, TileBase tile)
        {
            if (tilemap == null || bounds == null || tile == null) return;

            TileBase currentTile;
            Vector3Int position = new Vector3Int();

            int x, y;

            // Bottom horizontal bounds
            position.y = bounds.yMin;
            for (x = bounds.xMin; x<bounds.xMax; x++)
            {
                position.x = x;
                currentTile = tilemap.GetTile(position);
                if (currentTile != null)
                {
                    tilemap.SetTile(position, tile);
                }
            }

            // Top horizontal bounds
            position.y = bounds.yMax;
            for (x = bounds.xMin; x < bounds.xMax; x++)
            {
                position.x = x;
                currentTile = tilemap.GetTile(position);
                if (currentTile != null)
                {
                    tilemap.SetTile(position, tile);
                }
            }

            // Left vertical bounds
            position.x = bounds.xMin;
            for (y = bounds.yMin; x < bounds.yMax; y++)
            {
                position.y = y;
                currentTile = tilemap.GetTile(position);
                if (currentTile != null)
                {
                    tilemap.SetTile(position, tile);
                }
            }

            // Right vertical bounds
            position.x = bounds.xMax;
            for (y = bounds.yMin; x < bounds.yMax; y++)
            {
                position.y = y;
                currentTile = tilemap.GetTile(position);
                if (currentTile != null)
                {
                    tilemap.SetTile(position, tile);
                }
            }
        }

    }
}
