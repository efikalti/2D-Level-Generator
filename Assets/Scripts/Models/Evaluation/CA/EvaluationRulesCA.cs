using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models.Evaluation.CA
{
    public class EvaluationRulesCA : EvaluationRules
    {
        public EvaluationRulesCA(Tilemap tilemap)
        {
            Tilemap = tilemap;
            Results = new List<EvaluationResult>();
        }

        public void EvaluateCALevel()
        {
            // Evaluate bounds of tilemap are walls
            var boundsAreWallsResult = EvaluateBoundsAreOfType(Tilemap.cellBounds, Enums.TileType.WALL);
            Results.Add(boundsAreWallsResult);
            boundsAreWallsResult.PrintResult(nameof(boundsAreWallsResult));


            // Evaluate cells next to bounds of tilemap are corridors
            int corridorsXMin = Tilemap.cellBounds.xMin + 1;
            int corridorsYMin = Tilemap.cellBounds.yMin + 1;
            int sizeX = Tilemap.cellBounds.xMax - corridorsXMin - 1;
            int sizeY = Tilemap.cellBounds.yMax - corridorsYMin - 1;

            var corridorBounds = new BoundsInt(corridorsXMin, corridorsYMin, Tilemap.cellBounds.zMin, sizeX, sizeY, 0);
            var nextToBoundsAreCorridorsResult = EvaluateBoundsAreOfType(corridorBounds, Enums.TileType.CORRIDOR);
            Results.Add(nextToBoundsAreCorridorsResult);
            nextToBoundsAreCorridorsResult.PrintResult(nameof(nextToBoundsAreCorridorsResult));

            var numberOfRoomsResult = EvaluateRooms();
            Results.Add(numberOfRoomsResult);
            numberOfRoomsResult.PrintResult(nameof(numberOfRoomsResult));


            var roomAreasResult = EvaluateRoomAreas();
            Results.Add(roomAreasResult);
            roomAreasResult.PrintResult(nameof(roomAreasResult));

            TotalScore();
        }

    }
}
