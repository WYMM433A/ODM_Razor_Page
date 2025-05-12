namespace ODMRazor.Models
{

    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Lock { get; set; }
    }

    public class Item
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string Size { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class Agent
    {
        public int AgentID { get; set; }
        public string AgentName { get; set; }
        public string Address { get; set; }
    }

    public class Order
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public int AgentID { get; set; }
        public Agent Agent { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }

    public class OrderDetail
    {
        public int ID { get; set; }
        public int OrderID { get; set; }
        public int ItemID { get; set; }
        public int Quantity { get; set; }

        public Item Item { get; set; }
        public Order Order { get; set; }
    }
}
