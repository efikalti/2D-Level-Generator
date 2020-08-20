using Assets.Scripts.Models;
using System.Collections.Generic;

namespace Assets.Scripts.Evaluation.PathFindingEvaluation
{
    public class RoomArea
    {
        public List<TileObject> tiles;

        public RoomArea()
        {
            tiles = new List<TileObject>();
        }
    }
}
