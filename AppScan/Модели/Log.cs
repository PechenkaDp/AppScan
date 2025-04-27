using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppScan
{
    public class Log
    {
        public int LogId { get; set; }
        public DateTime Timestamp { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }
        public int? UserId { get; set; }
        public int? AssetId { get; set; }
    }
}
