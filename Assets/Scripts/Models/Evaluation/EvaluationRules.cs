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

        protected List<BoundsInt> FindRooms()
        {
            List<BoundsInt> roomAreas = new List<BoundsInt>();
            // Evaluate the area for rooms
            int corridorsXMin = Tilemap.cellBounds.xMin + 2;
            int corridorsYMin = Tilemap.cellBounds.yMin + 2;
            int sizeX = Tilemap.cellBounds.xMax - corridorsXMin - 2;
            int sizeY = Tilemap.cellBounds.yMax - corridorsYMin - 2;
            var roomAreaBounds = new BoundsInt(corridorsXMin, corridorsYMin, Tilemap.cellBounds.zMin, sizeX, sizeY, 0);

            int nextPositionToCheck = 0;
            Vector3Int position = new Vector3Int(0, 0, 0);
            for (int x = roomAreaBounds.xMin; x < roomAreaBounds.xMax; x++)
            {
                position.x = x;
                for (int y = roomAreaBounds.yMin; y < roomAreaBounds.yMax; y++)
                {
                    position.y = y;

                    if (IsTileOfType(position, Enums.TileType.WALL))
                    {
                        // Found wall tile
                        if (IsRoom(position, roomAreaBounds, out BoundsInt roomBounds, out nextPositionToCheck))
                        {
                            roomAreas.Add(roomBounds);
                        }
                    }
                }
            }

            return roomAreas;
        }

        protected bool IsRoom(Vector3Int startPosition, BoundsInt areaBounds, out BoundsInt roomBounds, out int nextPositionToCheck)
        {
            bool foundFloor = false;
            Vector3Int currentPosition = new Vector3Int(startPosition.x, startPosition.y + 1, startPosition.z);

            roomBounds = new BoundsInt();

            // Check vertical for the room bounds
            while (currentPosition.y <= areaBounds.yMax && !foundFloor)
            {
                if (!IsTileOfType(currentPosition, Enums.TileType.WALL))
                {
                    foundFloor = true;
                    currentPosition.y--;
                }
                else
                {
                    currentPosition.y++;
                }
            }
            int roomBoundsY = currentPosition.y;
            nextPositionToCheck = roomBoundsY + 1;

            if (!foundFloor || roomBoundsY == startPosition.y)
            {
                return false;
            }

            // Check horizontal for the room bounds
            foundFloor = false;
            currentPosition.x++;
            while (currentPosition.x <= areaBounds.xMax && !foundFloor)
            {
                if (!IsTileOfType(currentPosition, Enums.TileType.WALL))
                {
                    foundFloor = true;
                    currentPosition.x--;
                }
                else
                {
                    currentPosition.x++;
                }
            }
            int roomBoundsX = currentPosition.x;

            if (!foundFloor || roomBoundsX == startPosition.x)
            {
                return false;
            }



            // Check vertical with the bounds found for y
            for (int y = startPosition.y; y <= roomBoundsY; y++)
            {
                currentPosition.y = y;
                if (!IsTileOfType(currentPosition, Enums.TileType.WALL))
                {
                    return false;
                }
            }

            // Check horizontal with the bounds found for y
            currentPosition.y = startPosition.y;
            for (int x = startPosition.x; x <= roomBoundsX; x++)
            {
                currentPosition.x = x;
                if (!IsTileOfType(currentPosition, Enums.TileType.WALL))
                {
                    return false;
                }
            }

            if (Mathf.Abs(startPosition.x - roomBoundsX) == 1 || Mathf.Abs(startPosition.y - roomBoundsY) == 1)
            {
                return false;
            }

            roomBounds.SetMinMax(new Vector3Int(startPosition.x, startPosition.y, 0),
                                 new Vector3Int(roomBoundsX, roomBoundsY, 0));

            return true;
        }

        protected EvaluationResult EvaluateRooms()
        {
            Rooms = FindRooms();

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
