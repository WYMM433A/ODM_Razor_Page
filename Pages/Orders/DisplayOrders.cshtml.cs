using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ODMRazor.Models;

namespace ODMRazor.Pages.Orders
{
    [Authorize(AuthenticationSchemes = "CookieAuth")]
    public class DisplayOrdersModel : PageModel
    {
        private readonly AppDbContext _context;

        public DisplayOrdersModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string AgentNameFilter { get; set; }

        private List<Order> _orders = new List<Order>(); // Initialize with an empty list
        public List<Order> Orders
        {
            get => _orders;
            set => _orders = value ?? new List<Order>(); // Ensure it's never null
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.Agent)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Item)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(AgentNameFilter))
                {
                    query = query.Where(o => o.Agent.AgentName.Contains(AgentNameFilter));
                }

                Orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
                Console.WriteLine($"Orders loaded: {Orders.Count} items");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading orders: {ex.Message}");
                Orders = new List<Order>(); // Fallback to empty list on error
            }

            return Page();
        }
    }
}