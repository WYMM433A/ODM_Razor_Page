using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ODMRazor.Models;

namespace ODMRazor.Pages.OrderDetails
{
    public class CreateModel : PageModel
    {
        private readonly ODMRazor.Models.AppDbContext _context;

        public CreateModel(ODMRazor.Models.AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["ItemID"] = new SelectList(_context.Items, "ItemID", "ItemID");
        ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID");
            return Page();
        }

        [BindProperty]
        public OrderDetail OrderDetail { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.OrderDetails.Add(OrderDetail);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
