using System;

namespace AppScan
{
    public class Asset
    {
        public int AssetId { get; set; }
        public int ModelId { get; set; }
        public string InventoryNumber { get; set; }
        public string SerialNumber { get; set; }
        public string AssetNumber { get; set; }
        public string Barcode { get; set; }
        public int? EmployeeId { get; set; }
        public string Department { get; set; }
        public int? RoomId { get; set; }
        public string Status { get; set; }
        public DateTime? IssueDate { get; set; }
        public string AccountingInfo { get; set; }

        public string AssetModel { get; set; }
        public string ManufacturerName { get; set; }   
        public string AssetTypeName { get; set; }  
        public string RoomNumber { get; set; }  
        public string EmployeeName { get; set; }  
    }
}