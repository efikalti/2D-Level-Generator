using Assets.Scripts.Enums;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models
{
    public interface ITransformRule
    {
        TileType? Apply(TileBase[] neighbors);
        TileType? Apply(Vector3Int position, BoundsInt bounds);
    }

    public class TransformToTileFromBounds : ITransformRule
    {
        public TileType type { get; set; }

        public TransformToTileFromBounds(TileType type)
        {
            this.type = type;
        }

        public TileType? Apply(Vector3Int position, BoundsInt bounds)
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

        public TileType? Apply(TileBase[] neighbors)
        {
            throw new NotImplementedException();
        }
    }
    public class TransformToWallFromAdjacents : ITransformRule
    {

        public TileType? Apply(TileBase[] neighbors)
        {
            if (neighbors.Length < 9)
            {
                return null;
            }
            // Get middle tile
            var tile = neighbors[(int)TilePositions.MIDDLE];
            // Check if the tile is already a wall
            if (GetTileTypeFromSpriteName(tile.name) == TileType.WALL)
            {
                return null;
            }

            // Get horizontal and vertical adjacent tiles
            var adjacents = new TileBase[4] {
                neighbors[(int)TilePositions.MIDDLE_LEFT],
                neighbors[(int)TilePositions.MIDDLE_RIGHT],
                neighbors[(int)TilePositions.TOP_MIDDLE],
                neighbors[(int)TilePositions.DOWN_MIDDLE]
            };

            int count = 0;
            foreach(var adjacentTile in adjacents)
            {
                if (adjacentTile != null)
                {
                    count += GetTileTypeFromSpriteName(adjacentTile.name) == TileType.WALL ?
                        1 : 0;
                }
            }

            if (count >= 2)
            {
                return TileType.WALL;
            }

            return null;
        }

        public TileType? Apply(Vector3Int position, BoundsInt bounds)
        {
            throw new NotImplementedException();
        }

        public TileType GetTileTypeFromSpriteName(string name)
        {
            var defaultType = TileType.CORRIDOR;
            if (string.IsNullOrWhiteSpace(name))
            {
                return defaultType;
            }

            if (name.Equals(TileName.Room))
            {
                return TileType.ROOM;
            }
            else if(name.Equals(TileName.Floor))
            {
                return TileType.CORRIDOR;
            }
            else
            {
                return defaultType;
            }
        }
    }
    public class TransformToWallForRoom : ITransformRule
    {
        public TileType? Apply(TileBase[] neighbors)
        {
            if (neighbors.Length < 9)
            {
                return null;
            }
            // Get middle tile
            var tile = neighbors[(int)TilePositions.MIDDLE];

            var tileType = TilemapHelper.GetTileTypeFromSpriteName(tile.name);
            // Check if the tile is already a wall
            if (tileType == TileType.WALL)
            {
                return null;
            }
            // Check if the tile is not floor
            if (tileType != TileType.ROOM)
            {
                return null;
            }

            var neighborTypes = new TileType[8];
            int index = 0;
            int count = 0;
            foreach(var neighbor in neighbors)
            {
                if (count != (int)TilePositions.MIDDLE)
                {
                    neighborTypes[index] = TilemapHelper.GetTileTypeFromSpriteName(neighbor.name);
                    index++;
                }
                count++;
            }

            if (TilemapHelper.IsNextToType(neighborTypes, TileType.CORRIDOR))
            {
                return TileType.WALL;
            }

            return null;
        }

        public TileType? Apply(Vector3Int position, BoundsInt bounds)
        {
            throw new NotImplementedException();
        }
    }


    public class TransformToRoomFromAdjacents : ITransformRule
    {

        public TileType? Apply(TileBase[] neighbors)
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
                    count != (int)TilePositions.MIDDLE)
                {
                    var tileType = GetTileTypeFromSpriteName(neighbor.name);
                    if (tileType == TileType.ROOM)
                    {
                        roomCount++;
                    }
                }
                count++;
            }
            if (roomCount >= count / 2)
            {
                return TileType.ROOM;
            }
            return null;
        }

        public TileType? Apply(Vector3Int position, BoundsInt bounds)
        {
            throw new NotImplementedException();
        }

        public TileType GetTileTypeFromSpriteName(string name)
        {
            var defaultType = TileType.CORRIDOR;
            if (string.IsNullOrWhiteSpace(name))
            {
                return defaultType;
            }

            if (name.Equals(TileName.Room))
            {
                return TileType.ROOM;
            }
            else if (name.Equals(TileName.Floor))
            {
                return TileType.CORRIDOR;
            }
            else
            {
                return defaultType;
            }
        }
    }
}
