using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class TileObject
    {
        public Vector3Int Position;
        public TileType Type;

        public TileObject() { }

        public TileObject(Vector3Int position, TileType type) 
        {
            Position = position;
            Type = type;
        }
    }
}
