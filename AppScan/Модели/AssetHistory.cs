using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppScan
{
    public class AssetHistory
    {
        public int HistoryId { get; set; }
        public int AssetId { get; set; }
        public int? PreviousEmployeeId { get; set; }
        public int? NewEmployeeId { get; set; }
        public DateTime TransferDate { get; set; }
        public string RequestNumber { get; set; }
        public string Reason { get; set; }
    }
}
