using Assets.Scripts.Evaluation.PathFindingEvaluation;
using Assets.Scripts.Evaluation.RoomFindingEvaluation;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Generators
{
    public class PathOpeningGenerator
    {
        private RoomFindingEvaluator roomFindingEvaluator;

        public PathOpeningGenerator()
        {
        }

        public Tilemap OpenPathsBetweenRooms(Tilemap tilemap)
        {
            roomFindingEvaluator = new RoomFindingEvaluator(tilemap);
            roomFindingEvaluator.FindRooms();
            var rooms = roomFindingEvaluator.rooms;

            return tilemap;
        }
    }
}
