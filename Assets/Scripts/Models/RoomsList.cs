using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    public class RoomsList
    {
        public List<Room> Rooms { get; set; }

        public int MaxHeight { get; set; }

        public int MaxWidth { get; set; }

        public RoomsList() {
            Rooms = new List<Room>();
        }

        public RoomsList(List<Room> roomsList, int maxWidth, int maxHeight)
        {
            Rooms = roomsList;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
        }
    }
}
