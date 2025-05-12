namespace ODMRazor.DTOs
{
    public class OrderReportDTO
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string AgentName { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}