using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ODMRazor.Models;

namespace ODMRazor.Pages.Items
{

    [Authorize(AuthenticationSchemes = "CookieAuth")]
    public class IndexModel : PageModel
    {
        private readonly ODMRazor.Models.AppDbContext _context;

        public IndexModel(ODMRazor.Models.AppDbContext context)
        {
            _context = context;
        }

        public IList<Item> Item { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Item = await _context.Items.ToListAsync();
        }
    }
}
