using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class TileObject
    {
        public Vector3Int Position;
        public TILE_TYPE Type;

        public TileObject() { }

        public TileObject(Vector3Int position, TILE_TYPE type) 
        {
            Position = position;
            Type = type;
        }
    }
}
