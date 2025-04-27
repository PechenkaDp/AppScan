using System;

namespace AppScan
{
    public class SupportTicket
    {
        public int TicketId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public int? AssignedTo { get; set; }
        public string AssignedToName { get; set; }
        public string Response { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}