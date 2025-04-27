using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppScan
{
    public class AssetModel
    {
        public int ModelId { get; set; }
        public int ManufacturerId { get; set; }
        public int AssetTypeId { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }
        public string Specifications { get; set; }
        public string ManufacturerName { get; set; }
        public string AssetTypeName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
