using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;

namespace TicTacToeProServer.Pages
{
    public class IndexModel : PageModel
    {
        public List<KeyValuePair<HubCallerContext, Game>> playersInGame { get; set; } = GameHub.playersInGame.ToList();
        public int counter { get; set; }
        public HubCallerContext[] queue { get; set; } = GameHub.playersInQueue.ToArray();
        public IActionResult OnGet()
        {
            SetCounter();

            return Page();
        }

        public void SetCounter()
        {
            counter = playersInGame.Count / 2;
        }

        public void RemoveGame(Game game)
        {
            playersInGame.Remove(new KeyValuePair<HubCallerContext, Game>(game.X, game));
            playersInGame.Remove(new KeyValuePair<HubCallerContext, Game>(game.O, game));
        }
    }
}
