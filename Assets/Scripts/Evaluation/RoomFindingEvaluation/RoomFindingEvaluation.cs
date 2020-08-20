using Assets.Scripts.Enums;
using Assets.Scripts.Evaluation.PathFindingEvaluation;
using Assets.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Evaluation.RoomFindingEvaluation
{
    public class RoomFindingEvaluation
    {
        public Tilemap tilemap;

        private Hashtable searchedTiles;

        public List<RoomArea> rooms;

        public RoomFindingEvaluation(Tilemap t)
        {
            tilemap = t;

            searchedTiles = new Hashtable();
            rooms = new List<RoomArea>();
        }

        public void Evaluate()
        {
            var position = Vector3Int.zero;

            var bounds = tilemap.cellBounds;

            searchedTiles = new Hashtable();

            for (int y = bounds.yMin; y<bounds.yMax; y++)

            {
                position.y = y;
                for (int x = bounds.xMin; x<bounds.xMax; x++)
                {
                    position.x = x;

                    if (!searchedTiles.ContainsKey(position))
                    {
                        var currentTileType = TilemapHelper.GetTileTypeFromPosition(position, tilemap);
                        if (currentTileType == TileType.ROOM)
                        {
                            var room = FindRoomArea(position);
                            if (room.tiles.Any())
                            {
                                rooms.Add(room);
                            }
                        }
                    }

                }
            }

            PaintRooms();
            Debug.Log($"Number of room areas for this tilemap: {rooms.Count}");
            Debug.Log($"Searched: {searchedTiles.Count} tiles");

        }

        public RoomArea FindRoomArea(Vector3Int startPosition)
        {
            Queue<Vector3Int> tilesToCheck = new Queue<Vector3Int>();
            tilesToCheck.Enqueue(startPosition);

            RoomArea room = new RoomArea();

            while (tilesToCheck.Any())
            {
                var position = tilesToCheck.Dequeue();

                if (!searchedTiles.ContainsKey(position))
                {
                    searchedTiles.Add(position, position);

                    var tileType = TilemapHelper.GetTileTypeFromPosition(position, tilemap);

                    if (tileType == TileType.ROOM)
                    {
                        // Add current tile to room
                        room.tiles.Add(new TileObject(position, tileType));

                        tilesToCheck = AddNeighborsInQueue(tilesToCheck, position);

                    }
                }
            }

            return room;
        }

        public Queue<Vector3Int> AddNeighborsInQueue(Queue<Vector3Int> tilesToCheck, Vector3Int position)
        {
            // Add up position
            var upPosition = new Vector3Int(position.x, position.y + 1, position.z);
            var upTileType = TilemapHelper.GetTileTypeFromPosition(upPosition, tilemap);
            if (!searchedTiles.ContainsKey(upPosition))
            {
                tilesToCheck.Enqueue(upPosition);
            }

            // Add right position
            var rightPosition = new Vector3Int(position.x + 1, position.y, position.z);
            var rightTileType = TilemapHelper.GetTileTypeFromPosition(rightPosition, tilemap);
            if (!searchedTiles.ContainsKey(rightPosition))
            {
                tilesToCheck.Enqueue(rightPosition);
            }

            // Add down position
            var downPosition = new Vector3Int(position.x, position.y - 1, position.z);
            var downTileType = TilemapHelper.GetTileTypeFromPosition(downPosition, tilemap);
            if (!searchedTiles.ContainsKey(downPosition))
            {
                tilesToCheck.Enqueue(downPosition);
            }

            // Add left position
            var leftPosition = new Vector3Int(position.x - 1, position.y, position.z);
            var leftTileType = TilemapHelper.GetTileTypeFromPosition(leftPosition, tilemap);
            if (!searchedTiles.ContainsKey(leftPosition))
            {
                tilesToCheck.Enqueue(leftPosition);
            }

            if (upTileType == TileType.ROOM || rightTileType == TileType.ROOM)
            {
                // Add up and right position
                var uprightPosition = new Vector3Int(position.x + 1, position.y + 1, position.z);
                if (!searchedTiles.ContainsKey(uprightPosition))
                {
                    tilesToCheck.Enqueue(uprightPosition);
                }
            }

            if (downTileType == TileType.ROOM || rightTileType == TileType.ROOM)
            {
                // Add position down and right
                var downrightPosition = new Vector3Int(position.x + 1, position.y - 1, position.z);
                if (!searchedTiles.ContainsKey(downrightPosition))
                {
                    tilesToCheck.Enqueue(downrightPosition);
                }
            }

            if (downTileType == TileType.ROOM || leftTileType == TileType.ROOM)
            {
                // Add position down and left
                var downleftPosition = new Vector3Int(position.x - 1, position.y - 1, position.z);
                if (!searchedTiles.ContainsKey(downleftPosition))
                {
                    tilesToCheck.Enqueue(downleftPosition);
                }
            }

            if (upTileType == TileType.ROOM || leftTileType == TileType.ROOM)
            {
                // Add position up and left
                var upleftPosition = new Vector3Int(position.x - 1, position.y + 1, position.z);
                if (!searchedTiles.ContainsKey(upleftPosition))
                {
                    tilesToCheck.Enqueue(upleftPosition);
                }
            }

            return tilesToCheck;
        }

        public void PaintRooms()
        {
            foreach(var room in rooms)
            {
                var roomColor = RandomColor();
                foreach(var tile in room.tiles)
                {
                    tilemap.SetColor(tile.Position, roomColor);
                }
            }
        }

        public Color RandomColor()
        {
            int index = Random.Range(0, 7);

            switch (index)
            {
                case 0:
                    return Color.black;
                case 1:
                    return Color.magenta;
                case 2:
                    return Color.green;
                case 3:
                    return Color.grey;
                case 4:
                    return Color.blue;
                case 5:
                    return Color.red;
                case 6:
                    return Color.cyan;
                default:
                    return Color.grey;
            }
        }
    }
}
