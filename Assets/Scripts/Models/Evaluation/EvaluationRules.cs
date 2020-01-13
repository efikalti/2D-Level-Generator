using Assets.Scripts.Enums;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models.Evaluation
{
    public abstract class EvaluationRules
    {
        protected TilemapHelper TilemapHelper;
        protected Tilemap Tilemap;

        protected bool IsTileOfType(Vector3Int position, TileType type)
        {
            if (position == null)
            {
                return false;
            }

            var tileType = TilemapHelper.GetTileTypeFromSpriteName(Tilemap.GetTile(position).name);

            return tileType == type;
        }

        protected int EvaluateTileType(Vector3Int position, TileType type)
        {
            if (IsTileOfType(position, type))
            {
                return +1;
            }

            return -1;
        }


        protected EvaluationResult EvaluateBoundsAreOfType(BoundsInt bounds, TileType type)
        {
            int sum = 0;
            int x, y;

            Vector3Int position = new Vector3Int();

            // Bottom horizontal bound
            position.y = bounds.yMin;
            for (x = bounds.xMin; x < bounds.xMax; x++)
            {
                position.x = x;
                sum += EvaluateTileType(position, type);
            }


            // Top horizontal bound
            position.y = bounds.yMax - 1;
            for (x = bounds.xMin; x < bounds.xMax; x++)
            {
                position.x = x;
                sum += EvaluateTileType(position, type);
            }


            // Left vertical bound
            position.x = bounds.xMin;
            for (y = bounds.yMin + 1; y < bounds.yMax - 1; y++)
            {
                position.y = y;
                sum += EvaluateTileType(position, type);
            }

            // Right vertical bound
            position.x = bounds.xMax - 1;
            for (y = bounds.yMin + 1; y < bounds.yMax - 1; y++)
            {
                position.y = y;
                sum += EvaluateTileType(position, type);
            }

            return new EvaluationResult(sum);
        }
    }
}
