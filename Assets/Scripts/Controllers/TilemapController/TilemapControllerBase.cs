using Assets.Scripts.Models;
using Assets.Scripts.Reporting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Controllers.TilemapController
{
    public class TilemapControllerBase : MonoBehaviour
    {
        // GameObjects for Tilemaps
        public GameObject Walls;
        public GameObject Floors;
        public GameObject Corridors;

        // Array with tile assignments
        public TileItem[] TilesArray;

        // Tilemap main object
        protected Tilemap tilemap;

        // TODO: Maybe remove 
        // Tilemaps
        protected Tilemap WallTilemap;
        protected Tilemap FloorTilemap;
        protected Tilemap CorridorTilemap;

        protected TilemapHelper tilemapHelper;

        protected System.Random randomGenerator = new System.Random();

        protected DataParser fileParser;
    }
}
