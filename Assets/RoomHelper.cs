using Assets.Scripts;
using Assets.Scripts.Enums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets
{
    public static class RoomHelper
    {
        public static List<BoundsInt> FindRooms(Tilemap Tilemap)
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

                    var tile = Tilemap.GetTile(position);
                    if (TilemapHelper.IsTileOfType(tile.name, TileType.WALL))
                    {
                        if (IsRoom(Tilemap, position, roomAreaBounds, out BoundsInt roomBounds, out nextPositionToCheck))
                        {
                            roomAreas.Add(roomBounds);
                        }
                    }
                    
                }
            }

            return roomAreas;
        }


        public static bool IsRoom(Tilemap Tilemap, Vector3Int startPosition, BoundsInt areaBounds, out BoundsInt roomBounds, out int nextPositionToCheck)
        {
            bool foundFloor = false;
            Vector3Int currentPosition = new Vector3Int(startPosition.x, startPosition.y + 1, startPosition.z);

            roomBounds = new BoundsInt();

            TileBase tile;

            // Check vertical for the room bounds
            while (currentPosition.y <= areaBounds.yMax && !foundFloor)
            {
                tile = Tilemap.GetTile(currentPosition);
                if (!TilemapHelper.IsTileOfType(tile?.name, TileType.WALL))
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
                tile = Tilemap.GetTile(currentPosition);
                if (!TilemapHelper.IsTileOfType(tile?.name, TileType.WALL))
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
                tile = Tilemap.GetTile(currentPosition);
                if (!TilemapHelper.IsTileOfType(tile?.name, TileType.WALL))
                {
                    return false;
                }
            }

            // Check horizontal with the bounds found for y
            currentPosition.y = startPosition.y;
            for (int x = startPosition.x; x <= roomBoundsX; x++)
            {
                currentPosition.x = x;
                tile = Tilemap.GetTile(currentPosition);
                if (!TilemapHelper.IsTileOfType(tile?.name, TileType.WALL))
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

    }
}
