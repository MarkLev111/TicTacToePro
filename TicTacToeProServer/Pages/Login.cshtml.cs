using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TicTacToeProServer.Pages
{
    public class LoginModel : PageModel
    {
        public string message { get; set; } = "";

        [BindProperty]
        public string? logUsername { get; set; }

        [BindProperty]
        public string? logPassword { get; set; }

        private readonly DBContext dbContext;
        private readonly IConfiguration config;
        public LoginModel(DBContext context, IConfiguration config)
        {
            dbContext = context;
            this.config = config;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if (logUsername != "MarkLev111")
                return Unauthorized();
            else
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.username == logUsername);
                bool passwordCheck = BCrypt.Net.BCrypt.Verify(logPassword, user.password);
                if (!passwordCheck)
                    return Unauthorized();
                HttpContext.Session.SetString("LoggedIn", "true");
                return RedirectToPage("/index");
            }
        }
    }
}
