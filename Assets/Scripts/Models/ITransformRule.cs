using Assets.Scripts.Enums;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models
{
    public interface ITransformRule
    {
        TILE_TYPE? Apply(TileBase[] neighbors);
        TILE_TYPE? Apply(Vector3Int position, BoundsInt bounds);
    }

    public class TransformToTileFromBounds : ITransformRule
    {
        public TILE_TYPE type { get; set; }

        public TransformToTileFromBounds(TILE_TYPE type)
        {
            this.type = type;
        }

        public TILE_TYPE? Apply(Vector3Int position, BoundsInt bounds)
        {
            if (bounds.x == position.x ||
                bounds.y == position.y ||
                bounds.xMax == position.x + 1 ||
                bounds.yMax == position.y + 1)
            {
                return type;
            }
            return null;
        }

        public TILE_TYPE? Apply(TileBase[] neighbors)
        {
            throw new NotImplementedException();
        }
    }
    public class TransformToWallFromAdjacents : ITransformRule
    {

        public TILE_TYPE? Apply(TileBase[] neighbors)
        {
            if (neighbors.Length < 9)
            {
                return null;
            }
            // Get middle tile
            var tile = neighbors[(int)TILE_POSITIONS.MIDDLE];
            // Check if the tile is already a wall
            if (GetTileTypeFromSpriteName(tile.name) == TILE_TYPE.WALL)
            {
                return null;
            }

            // Get horizontal and vertical adjacent tiles
            var adjacents = new TileBase[4] {
                neighbors[(int)TILE_POSITIONS.MIDDLE_LEFT],
                neighbors[(int)TILE_POSITIONS.MIDDLE_RIGHT],
                neighbors[(int)TILE_POSITIONS.TOP_MIDDLE],
                neighbors[(int)TILE_POSITIONS.DOWN_MIDDLE]
            };

            int count = 0;
            foreach(var adjacentTile in adjacents)
            {
                if (adjacentTile != null)
                {
                    count += GetTileTypeFromSpriteName(adjacentTile.name) == TILE_TYPE.WALL ?
                        1 : 0;
                }
            }

            if (count >= 2)
            {
                return TILE_TYPE.WALL;
            }

            return null;
        }

        public TILE_TYPE? Apply(Vector3Int position, BoundsInt bounds)
        {
            throw new NotImplementedException();
        }

        public TILE_TYPE GetTileTypeFromSpriteName(string name)
        {
            var defaultType = TILE_TYPE.CORRIDOR;
            if (string.IsNullOrWhiteSpace(name))
            {
                return defaultType;
            }

            if (name.Equals(TileName.Room))
            {
                return TILE_TYPE.ROOM_1;
            }
            else if(name.Equals(TileName.Floor))
            {
                return TILE_TYPE.CORRIDOR;
            }
            else
            {
                return defaultType;
            }
        }
    }

    public class TransformToRoomFromAdjacents : ITransformRule
    {

        public TILE_TYPE? Apply(TileBase[] neighbors)
        {
            if (neighbors.Length < 9)
            {
                return null;
            }
            int count = 0;
            int roomCount = 0;
            foreach (var neighbor in neighbors)
            {
                if (neighbor != null &&
                    count != (int)TILE_POSITIONS.MIDDLE)
                {
                    var tileType = GetTileTypeFromSpriteName(neighbor.name);
                    if (tileType == TILE_TYPE.ROOM_1)
                    {
                        roomCount++;
                    }
                }
                count++;
            }
            if (roomCount >= count / 2)
            {
                return TILE_TYPE.ROOM_1;
            }
            return null;
        }

        public TILE_TYPE? Apply(Vector3Int position, BoundsInt bounds)
        {
            throw new NotImplementedException();
        }

        public TILE_TYPE GetTileTypeFromSpriteName(string name)
        {
            var defaultType = TILE_TYPE.CORRIDOR;
            if (string.IsNullOrWhiteSpace(name))
            {
                return defaultType;
            }

            if (name.Equals(TileName.Room))
            {
                return TILE_TYPE.ROOM_1;
            }
            else if (name.Equals(TileName.Floor))
            {
                return TILE_TYPE.CORRIDOR;
            }
            else
            {
                return defaultType;
            }
        }
    }
}
