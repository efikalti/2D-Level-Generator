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
            Debug.Log(Tilemap.cellBounds);
            Debug.Log("Bounds are walls result: " + boundsAreWallsResult.Score);


            // Evaluate cells next to bounds of tilemap are corridors
            int corridorsXMin = Tilemap.cellBounds.xMin + 1;
            int corridorsYMin = Tilemap.cellBounds.yMin + 1;
            int sizeX = Tilemap.cellBounds.xMax - corridorsXMin - 1;
            int sizeY = Tilemap.cellBounds.yMax - corridorsYMin - 1;

            var corridorBounds = new BoundsInt(corridorsXMin, corridorsYMin, Tilemap.cellBounds.zMin, sizeX, sizeY, 0);
            var nextToBoundsAreCorridors = EvaluateBoundsAreOfType(corridorBounds, Enums.TileType.CORRIDOR);
            Debug.Log(corridorBounds);
            Debug.Log("Next to bounds are corridors result: " + nextToBoundsAreCorridors.Score);
        }

        public void EvaluateRooms()
        {

            // Evaluate the area for rooms
            int corridorsXMin = Tilemap.cellBounds.xMin + 2;
            int corridorsYMin = Tilemap.cellBounds.yMin + 2;
            int sizeX = Tilemap.cellBounds.xMax - corridorsXMin - 2;
            int sizeY = Tilemap.cellBounds.yMax - corridorsYMin - 2;
            var roomAreaBounds = new BoundsInt(corridorsXMin, corridorsYMin, Tilemap.cellBounds.zMin, sizeX, sizeY, 0);

            Vector3Int position = new Vector3Int(0, 0, 0);
            for(int x = roomAreaBounds.xMin; x< roomAreaBounds.xMax; x++)
            {
                position.x = x;
                for(int y = roomAreaBounds.yMin; y< roomAreaBounds.yMax; y++)
                {
                    position.y = y;

                    if (IsTileOfType(position, Enums.TileType.WALL)) {
                        // Found wall tile

                    }
                }
            }
        }

        public bool IsRoom(Vector3Int startPosition, BoundsInt areaBounds)
        {
            int xMin = startPosition.x;
            int yMin = startPosition.y;
            int xMax = xMin + 1;
            int yMax = yMin + 1;
            bool foundNotWall = false;
            Vector3Int currentPosition = new Vector3Int(startPosition.x, startPosition.y, startPosition.z);

            // Check vertical for the room bounds
            while (yMax < areaBounds.yMax && !foundNotWall)
            {
                if(!IsTileOfType(currentPosition, Enums.TileType.WALL))
                {
                    foundNotWall = true;
                    currentPosition.y--;
                }
                else
                {
                    currentPosition.y++;
                }
            }

            if (!foundNotWall)
            {
                return false;
            }

            int roomBoundsY = currentPosition.y;

            // Check horizontal for the room bounds
            foundNotWall = false;
            currentPosition.x++;
            while (xMax < areaBounds.xMax && !foundNotWall)
            {
                if (!IsTileOfType(currentPosition, Enums.TileType.WALL))
                {
                    foundNotWall = true;
                    currentPosition.x--;
                }
                else
                {
                    currentPosition.x++;
                }
            }

            if (!foundNotWall)
            {
                return false;
            }


            int roomBoundsX = currentPosition.x;

            // Check vertical with the bounds found for y
            for(int y=startPosition.y; y<=roomBoundsY; y++)
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

            return true;
        }

    }
}
