using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    public class Room
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Room() { }

        public Room(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
