using Assets.Scripts.Enums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models.Evaluation
{
    public abstract class EvaluationRules
    {
        protected Tilemap Tilemap;
        protected List<BoundsInt> Rooms;
        protected List<EvaluationResult> Results;

        protected void TotalScore()
        {
            int totalScore = 0;
            foreach(var result in Results)
            {
                totalScore += result.Score;
            }
            Debug.Log("Total room score: " + totalScore);
        }

        protected int EvaluateTileType(Vector3Int position, TileType type)
        {
            var tile = Tilemap.GetTile(position);
            if (TilemapHelper.IsTileOfType(tile?.name, type))
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


        protected int EvaluateTilesInBoundsAreOfType(BoundsInt bounds, TileType type)
        {
            int sum = 0;
            Vector3Int position = new Vector3Int
            {
                z = bounds.zMin
            };

            for (int x = bounds.xMin + 1; x < bounds.xMax - 1; x++)
            {
                position.x = x;
                for (int y = bounds.yMin + 1; y < bounds.yMax - 1; y++)
                {
                    position.y = y;
                    sum += EvaluateTileType(position, type);
                }
            }

            return sum;
        }

        protected EvaluationResult EvaluateRooms()
        {
            Rooms = RoomHelper.FindRooms(Tilemap);

            int score = 0;

            if (Rooms.Count > 3)
            {
                score = 100;
            }
            else if (Rooms.Count > 0)
            {
                score = 50;
            }

            return new EvaluationResult(score);
        }

        protected EvaluationResult EvaluateRoomAreas()
        {
            int sum = 0;
            foreach(var roomArea in Rooms)
            {
                sum += EvaluateTilesInBoundsAreOfType(roomArea, TileType.ROOM);
            }

            return new EvaluationResult(sum);
        }
    }
}
