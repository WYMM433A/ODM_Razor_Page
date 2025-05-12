using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ODMRazor.Models;

namespace ODMRazor.Pages.Agents
{
    public class DetailsModel : PageModel
    {
        private readonly ODMRazor.Models.AppDbContext _context;

        public DetailsModel(ODMRazor.Models.AppDbContext context)
        {
            _context = context;
        }

        public Agent Agent { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agent = await _context.Agents.FirstOrDefaultAsync(m => m.AgentID == id);
            if (agent == null)
            {
                return NotFound();
            }
            else
            {
                Agent = agent;
            }
            return Page();
        }
    }
}
