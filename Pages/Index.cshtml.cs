using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ODMRazor.Models;
using System.Linq;

namespace ODMRazor.Pages
{
    [Authorize(AuthenticationSchemes = "CookieAuth")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        // Best Selling Items
        public List<ItemSales> BestSellingItems { get; set; }

        // Items Purchased by Agent
        [BindProperty(SupportsGet = true)]
        public int? SelectedAgentId { get; set; }
        public List<AgentPurchaseDetail> ItemsByAgent { get; set; }
        public SelectList AgentList { get; set; }

        // Agents Who Purchased Specific Item
        [BindProperty(SupportsGet = true)]
        public int? SelectedItemId { get; set; }
        public List<ItemPurchaseDetail> AgentsByItem { get; set; }
        public SelectList ItemList { get; set; }

        public class ItemSales
        {
            public int ItemId { get; set; }
            public string ItemName { get; set; }
            public int TotalQuantity { get; set; }
        }

        public class AgentPurchaseDetail
        {
            public string ItemName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total { get; set; }
        }

        public class ItemPurchaseDetail
        {
            public string AgentName { get; set; }
            public DateTime OrderDate { get; set; }
            public int TotalQuantity { get; set; }
        }

        public async Task OnGetAsync()
        {
            // Best Selling Items of This Month
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            BestSellingItems = await _context.OrderDetails
                .Where(od => od.Order.OrderDate >= startOfMonth)
                .GroupBy(od => new { od.ItemID, od.Item.ItemName })
                .Select(g => new ItemSales
                {
                    ItemId = g.Key.ItemID,
                    ItemName = g.Key.ItemName,
                    TotalQuantity = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();

            // Populate Agent Dropdown
            AgentList = new SelectList(await _context.Agents.ToListAsync(), "AgentID", "AgentName", SelectedAgentId);

            // Items Purchased by Selected Agent
            if (SelectedAgentId.HasValue)
            {
                ItemsByAgent = await _context.OrderDetails
                    .Where(od => od.Order.AgentID == SelectedAgentId)
                    .GroupBy(od => new { od.Item.ItemName, od.Item.UnitPrice })
                    .Select(g => new AgentPurchaseDetail
                    {
                        ItemName = g.Key.ItemName,
                        Quantity = g.Sum(od => od.Quantity),
                        UnitPrice = g.Key.UnitPrice,
                        Total = g.Sum(od => od.Quantity) * g.Key.UnitPrice
                    })
                    .ToListAsync();
            }
            else
            {
                ItemsByAgent = new List<AgentPurchaseDetail>();
            }

            // Populate Item Dropdown
            ItemList = new SelectList(await _context.Items.ToListAsync(), "ItemID", "ItemName", SelectedItemId);

            // Agents Who Purchased Selected Item
            if (SelectedItemId.HasValue)
            {
                AgentsByItem = await _context.OrderDetails
                    .Where(od => od.ItemID == SelectedItemId)
                    .GroupBy(od => new { od.Order.Agent.AgentName, od.Order.OrderDate })
                    .Select(g => new ItemPurchaseDetail
                    {
                        AgentName = g.Key.AgentName,
                        OrderDate = g.Key.OrderDate,
                        TotalQuantity = g.Sum(od => od.Quantity)
                    })
                    .ToListAsync();
            }
            else
            {
                AgentsByItem = new List<ItemPurchaseDetail>();
            }
        }
    }
}