using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppScan
{
    public class Room
    {
        public int RoomId { get; set; }
        public int BuildingId { get; set; }
        public string RoomNumber { get; set; }
        public int Floor { get; set; }
        public string BuildingName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
