using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Models.Evaluation.PathFindingEvaluation
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
