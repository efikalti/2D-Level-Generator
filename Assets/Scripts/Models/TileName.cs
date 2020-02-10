﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    public class TileName
    {

        private TileName(string value) { Value = value; }

        public string Value { get; set; }

        public static TileName Floor { get { return new TileName("CorridorTile"); } }
        public static TileName Wall { get { return new TileName("WallTile"); } }
        public static TileName Room { get { return new TileName("RoomTile"); } }
        
    }
}
