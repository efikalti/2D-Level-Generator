using Assets.Scripts.Enums;
using System;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models
{
    [Serializable]
    public struct TileItem
    {
        public TILE_TYPE TileType;
        public TileBase Tile;
        public int Percentage;
    }
}
