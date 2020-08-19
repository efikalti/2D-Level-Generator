using Assets.Scripts.Enums;
using Assets.Scripts.Models.Evaluation.PathFindingEvaluation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models.Evaluation.RoomFindingEvaluation
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

            //var currentTileBase = tilemap.GetTile(position);
            //var tileType = TilemapHelper.GetTileTypeFromSpriteName(currentTileBase.name);
            //Debug.Log($"0,0: {tileType}");
            //position = new Vector3Int(99, 99, 0);
            //currentTileBase = tilemap.GetTile(position);
            //tileType = TilemapHelper.GetTileTypeFromSpriteName(currentTileBase.name);
            //Debug.Log($"99,99: {tileType}");
            //position = new Vector3Int(0, 99, 0);
            //currentTileBase = tilemap.GetTile(position);
            //tileType = TilemapHelper.GetTileTypeFromSpriteName(currentTileBase.name);
            //Debug.Log($"0,99: {tileType}");
            //position = new Vector3Int(99, 0, 0);
            //currentTileBase = tilemap.GetTile(position);
            //tileType = TilemapHelper.GetTileTypeFromSpriteName(currentTileBase.name);
            //Debug.Log($"99,0: {tileType}");

            var bounds = tilemap.cellBounds;
            for (int x=bounds.xMin; x<bounds.xMax; x++)
            {
                position.x = x;
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    position.y = y;

                    string key = $"{position.x}{position.y}{position.z}";
                    if (!searchedTiles.ContainsKey(key))
                    {
                        var currentTileBase = tilemap.GetTile(position);
                        if (currentTileBase != null)
                        {
                            var tileType = TilemapHelper.GetTileTypeFromSpriteName(currentTileBase.name);
                            if (tileType == TileType.ROOM)
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
            }

            Debug.Log($"Number of room areas for this tilemap: {rooms.Count}");
        }

        public RoomArea FindRoomArea(Vector3Int startPosition)
        {
            Queue<Vector3Int> tilesToCheck = new Queue<Vector3Int>();
            tilesToCheck.Enqueue(startPosition);

            RoomArea room = new RoomArea();

            while (tilesToCheck.Any())
            {
                var position = tilesToCheck.Dequeue();
                string key = $"{position.x}{position.y}{position.z}";

                if (!searchedTiles.ContainsKey(key))
                {
                    searchedTiles.Add(key, position);

                    var currentTileBase = tilemap.GetTile(position);
                    if (currentTileBase != null)
                    {
                        var tileType = TilemapHelper.GetTileTypeFromSpriteName(currentTileBase.name);

                        if (tileType == TileType.ROOM)
                        {
                            // Add current tile to room
                            room.tiles.Add(new TileObject(position, tileType));

                            // Add position in right
                            tilesToCheck.Enqueue(new Vector3Int(position.x + 1, position.y, position.z));

                            // Add position up
                            tilesToCheck.Enqueue(new Vector3Int(position.x, position.y + 1, position.z));

                            // Add position up and right
                            tilesToCheck.Enqueue(new Vector3Int(position.x + 1, position.y + 1, position.z));

                            // Add position down
                            tilesToCheck.Enqueue(new Vector3Int(position.x, position.y - 1, position.z));

                            // Add position down and right
                            tilesToCheck.Enqueue(new Vector3Int(position.x + 1, position.y - 1, position.z));
                        }
                    }
                }
            }

            return room;
        }
    }
}
