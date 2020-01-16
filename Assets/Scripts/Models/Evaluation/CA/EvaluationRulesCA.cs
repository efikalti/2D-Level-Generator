using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models.Evaluation.CA
{
    public class EvaluationRulesCA : EvaluationRules
    {


        public EvaluationRulesCA(Tilemap tilemap)
        {
            TilemapHelper = new TilemapHelper();
            Tilemap = tilemap;
        }

        public void EvaluateCALevel()
        {
            // Evaluate bounds of tilemap are walls
            var boundsAreWallsResult = EvaluateBoundsAreOfType(Tilemap.cellBounds, Enums.TileType.WALL);


            // Evaluate cells next to bounds of tilemap are corridors
            int corridorsXMin = Tilemap.cellBounds.xMin + 1;
            int corridorsYMin = Tilemap.cellBounds.yMin + 1;
            int sizeX = Tilemap.cellBounds.xMax - corridorsXMin - 1;
            int sizeY = Tilemap.cellBounds.yMax - corridorsYMin - 1;

            var corridorBounds = new BoundsInt(corridorsXMin, corridorsYMin, Tilemap.cellBounds.zMin, sizeX, sizeY, 0);
            var nextToBoundsAreCorridors = EvaluateBoundsAreOfType(corridorBounds, Enums.TileType.CORRIDOR);

            EvaluateRooms();
        }

        public void EvaluateRooms()
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
            for(int x = roomAreaBounds.xMin; x < roomAreaBounds.xMax; x++)
            {
                position.x = x;
                for(int y = roomAreaBounds.yMin; y < roomAreaBounds.yMax; y++)
                {
                    position.y = y;

                    if (IsTileOfType(position, Enums.TileType.WALL)) {
                        // Found wall tile
                        if(IsRoom(position, roomAreaBounds, out BoundsInt roomBounds, out nextPositionToCheck))
                        {
                            Debug.Log("count");
                            roomAreas.Add(roomBounds);
                        }
                    }
                }
            }

            PrintRoomAreas(roomAreas);
        }

        public bool IsRoom(Vector3Int startPosition, BoundsInt areaBounds, out BoundsInt roomBounds, out int nextPositionToCheck)
        {
            bool foundFloor = false;
            Vector3Int currentPosition = new Vector3Int(startPosition.x, startPosition.y + 1, startPosition.z);

            roomBounds = new BoundsInt();

            // Check vertical for the room bounds
            while (currentPosition.y <= areaBounds.yMax && !foundFloor)
            {
                if(!IsTileOfType(currentPosition, Enums.TileType.WALL))
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
            for(int y=startPosition.y; y <= roomBoundsY; y++)
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

        private void PrintRoomAreas(List<BoundsInt> roomAreas)
        {
            Debug.Log($"Number of rooms: {roomAreas.Count}");
            foreach (var area in roomAreas)
            {
                var str = $"Min: {area.min.ToString()}, Max: {area.max.ToString()}";
                Debug.Log(str);
            }

        }

    }
}
