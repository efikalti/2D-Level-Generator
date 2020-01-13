using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models.Evaluation.CA
{
    public class EvaluationRulesCA : EvaluationRules
    {


        public EvaluationRulesCA(Tilemap tilemap)
        {
            TilemapHelper = new TilemapHelper();
            Tilemap = tilemap;
        }

        public void EvaluateCALevel()
        {
            // Evaluate bounds of tilemap are walls
            var boundsAreWallsResult = EvaluateBoundsAreOfType(Tilemap.cellBounds, Enums.TileType.WALL);
            Debug.Log(Tilemap.cellBounds);
            Debug.Log("Bounds are walls result: " + boundsAreWallsResult.Score);


            // Evaluate cells next to bounds of tilemap are corridors
            int corridorsXMin = Tilemap.cellBounds.xMin + 1;
            int corridorsYMin = Tilemap.cellBounds.yMin + 1;
            int sizeX = Tilemap.cellBounds.xMax - corridorsXMin - 1;
            int sizeY = Tilemap.cellBounds.yMax - corridorsYMin - 1;

            var corridorBounds = new BoundsInt(corridorsXMin, corridorsYMin, Tilemap.cellBounds.zMin, sizeX, sizeY, 0);
            var nextToBoundsAreCorridors = EvaluateBoundsAreOfType(corridorBounds, Enums.TileType.CORRIDOR);
            Debug.Log(corridorBounds);
            Debug.Log("Next to bounds are corridors result: " + nextToBoundsAreCorridors.Score);
        }

    }
}
