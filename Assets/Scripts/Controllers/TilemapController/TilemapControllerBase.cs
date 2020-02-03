using Assets.Scripts.Models;
using Assets.Scripts.Reporting;
using System.Collections;
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

        // Level generation parameters
        public int numberOfDungeonsToGenerate = 1;
        public int secondsToWaitingBetweenDungeons = 3;

        public void StartGenerating()
        {
            StartCoroutine(GenerateLevels());
        }

        public IEnumerator GenerateLevels()
        {
            WaitForSeconds wait = new WaitForSeconds(secondsToWaitingBetweenDungeons);
            for (int i = 0; i < numberOfDungeonsToGenerate; i++)
            {
                GenerateLevel();
                yield return wait;
            }
            StopCoroutine(GenerateLevels());
        }

        public virtual void GenerateLevel()
        {

        }

        /// <summary>
        /// Fill the bounded tilemap area with one type of tile
        /// </summary>
        /// <param name="bounds">The bounded tilemap area to fill with this tile</param>
        public void FillAreaWithTile(BoundsInt bounds, TileBase tile)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }
}
