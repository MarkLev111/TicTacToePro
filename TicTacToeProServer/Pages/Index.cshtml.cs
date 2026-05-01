using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TicTacToeProServer.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            var isLoggedIn = HttpContext.Session.GetString("LoggedIn");

            if (isLoggedIn != "true")
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }
    }
}
