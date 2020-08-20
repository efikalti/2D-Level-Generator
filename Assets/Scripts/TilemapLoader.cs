using Assets.Scripts.Generators;
using Assets.Scripts.Models;
using Assets.Scripts.Reporting;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public class TilemapLoader : TilemapGeneratorBase
    {
        public void Start()
        {
            SetupTilemapLoad();
        }

        public void SetupTilemapLoad()
        {
            // Create tilemap if it does not exist
            tilemap = GetComponent<Tilemap>();
            if (tilemap == null)
            {
                tilemap = transform.gameObject.AddComponent<Tilemap>();
                transform.gameObject.AddComponent<TilemapRenderer>();
            }

            // Create GraphParser object
            fileParser = new DataParser();

        }

        public void LoadTilemapFromList(List<TileObject> tiles)
        {
            foreach (var tile in tiles)
            {
                tilemap.SetTile(tile.Position, TilemapHelper.GetTileByType(tile.Type));
            }
        }

        public void LoadNextTilemapFromFile()
        {
            var tiles = fileParser.LoadNextFile();
            foreach (var tile in tiles)
            {
                tilemap.SetTile(tile.Position, TilemapHelper.GetTileByType(tile.Type));
            }
        }

        public void LoadPreviousTilemapFromFile()
        {
            var tiles = fileParser.LoadPreviousFile();
            foreach (var tile in tiles)
            {
                tilemap.SetTile(tile.Position, TilemapHelper.GetTileByType(tile.Type));
            }
        }

        public void LoadInputFiles(string folder)
        {
            fileParser.LoadInputFiles(folder);
        }
    }
}
