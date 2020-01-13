using Assets.Scripts.Enums;
using System;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models
{
    [Serializable]
    public struct TileItem
    {
        public TileType TileType;
        public TileBase Tile;
        public int Percentage;
    }
}
