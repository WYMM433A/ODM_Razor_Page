using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ODMRazor.Models;
using System.ComponentModel.DataAnnotations;

namespace ODMRazor.Pages.Orders
{
    [Authorize(AuthenticationSchemes = "CookieAuth")]
    public class CreateOrderModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateOrderModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public OrderInputModel OrderInput { get; set; }

        public SelectList AgentList { get; set; } = new SelectList(Enumerable.Empty<Agent>(), "AgentID", "AgentName"); // Initialize with empty list
        public SelectList ItemList { get; set; } = new SelectList(Enumerable.Empty<Item>(), "ItemID", "ItemName"); // Initialize with empty list

        [BindProperty(SupportsGet = true)]
        public int? OrderId { get; set; }

        public class OrderInputModel
        {
            public int? OrderID { get; set; }
            [Required]
            public int AgentID { get; set; }
            public List<OrderDetailInputModel> OrderDetails { get; set; } = new List<OrderDetailInputModel>();
        }

        public class OrderDetailInputModel
        {
            public int? ID { get; set; }
            [Required]
            public int ItemID { get; set; }
            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
            public int Quantity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                AgentList = new SelectList(await _context.Agents.ToListAsync(), "AgentID", "AgentName");
                ItemList = new SelectList(await _context.Items.ToListAsync(), "ItemID", "ItemName");
                Console.WriteLine($"OnGetAsync: Loaded {((SelectList)AgentList).Count()} agents, {((SelectList)ItemList).Count()} items");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnGetAsync Error: {ex.Message}");
                AgentList = new SelectList(Enumerable.Empty<Agent>(), "AgentID", "AgentName");
                ItemList = new SelectList(Enumerable.Empty<Item>(), "ItemID", "ItemName");
            }

            OrderInput = new OrderInputModel
            {
                OrderDetails = new List<OrderDetailInputModel> { new OrderDetailInputModel() }
            };

            if (OrderId.HasValue)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Item)
                    .FirstOrDefaultAsync(o => o.OrderID == OrderId);
                if (order != null)
                {
                    OrderInput.OrderID = order.OrderID;
                    OrderInput.AgentID = order.AgentID;
                    OrderInput.OrderDetails = order.OrderDetails
                        .Select(od => new OrderDetailInputModel
                        {
                            ID = od.ID,
                            ItemID = od.ItemID,
                            Quantity = od.Quantity
                        }).ToList();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateOrUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                await LoadDropdownsAsync();
                return Page();
            }

            Order order;
            if (OrderInput.OrderID.HasValue)
            {
                order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderID == OrderInput.OrderID.Value);
                if (order == null) return NotFound();

                order.AgentID = OrderInput.AgentID;
                _context.OrderDetails.RemoveRange(order.OrderDetails);
                Console.WriteLine($"Updating order {order.OrderID} with AgentID {order.AgentID}");
            }
            else
            {
                order = new Order
                {
                    OrderDate = DateTime.Now,
                    AgentID = OrderInput.AgentID
                };
                Console.WriteLine($"Creating new order with AgentID {order.AgentID}");

                _context.Orders.Add(order);
                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"New order saved with OrderID {order.OrderID}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save new order: {ex.InnerException?.Message ?? ex.Message}");
                    await LoadDropdownsAsync();
                    return Page();
                }

                order = await _context.Orders.FindAsync(order.OrderID);
            }

            var newDetails = OrderInput.OrderDetails
                .Where(od => od.ItemID > 0 && od.Quantity > 0)
                .Select(od => new OrderDetail
                {
                    ID = 0,
                    OrderID = order.OrderID,
                    ItemID = od.ItemID,
                    Quantity = od.Quantity
                }).ToList();

            if (!newDetails.Any())
            {
                ModelState.AddModelError(string.Empty, "At least one valid order detail is required.");
                await LoadDropdownsAsync();
                return Page();
            }

            Console.WriteLine($"Adding {newDetails.Count} order details for OrderID {order.OrderID}:");
            foreach (var detail in newDetails)
            {
                Console.WriteLine($"Detail: OrderID={detail.OrderID}, ItemID={detail.ItemID}, Quantity={detail.Quantity}");
                _context.OrderDetails.Add(detail);
            }

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Order details saved successfully for OrderID {order.OrderID}");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException when saving order details: {ex.InnerException?.Message ?? ex.Message}");
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    Console.WriteLine($"Entity: {entry.Entity}, State: {entry.State}");
                }
                await LoadDropdownsAsync();
                return Page();
            }

            return RedirectToPage("/Orders/DisplayOrders");
        }

        public async Task<IActionResult> OnPostDeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);
            if (order != null)
            {
                _context.OrderDetails.RemoveRange(order.OrderDetails);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("/Orders/DisplayOrders");
        }

        public async Task<IActionResult> OnPostDeleteOrderDetailAsync(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail != null)
            {
                int orderId = orderDetail.OrderID;
                _context.OrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();
                return RedirectToPage("/Orders/CreateOrder", new { orderId });
            }
            return RedirectToPage("/Orders/DisplayOrders");
        }

        private async Task LoadDropdownsAsync()
        {
            try
            {
                AgentList = new SelectList(await _context.Agents.ToListAsync(), "AgentID", "AgentName");
                ItemList = new SelectList(await _context.Items.ToListAsync(), "ItemID", "ItemName");
                Console.WriteLine($"LoadDropdownsAsync: Loaded {((SelectList)AgentList).Count()} agents, {((SelectList)ItemList).Count()} items");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadDropdownsAsync Error: {ex.Message}");
                AgentList = new SelectList(Enumerable.Empty<Agent>(), "AgentID", "AgentName");
                ItemList = new SelectList(Enumerable.Empty<Item>(), "ItemID", "ItemName");
            }
        }
    }
}