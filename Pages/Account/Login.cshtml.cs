using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ODMRazor.Models;
using System.Security.Claims;
namespace ODMRazor.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _context;
        [BindProperty]
        public InputModel Input { get; set; }
        public LoginModel(AppDbContext context)
        {
            _context = context;
        }
        public class InputModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == Input.Email && u.Password == Input.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return Page();
            }
            if (user.Lock)
            {
                ModelState.AddModelError(string.Empty, "Account is locked.");
                return Page();
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal, new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            });
            return RedirectToPage("/Index");
        }
    }
}