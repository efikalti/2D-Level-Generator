using Assets.Scripts.Enums;
using System;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models
{
    public class TileItem
    {
        public TileType TileType { get; set; }
        public TileBase Tilebase { get; set; }

        public TileItem()
        {

        }
    }
}
