using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TicTacToeProServer
{
    public class NotInGameRequirement : IAuthorizationRequirement { } // просто требование, что игрок не должен быть уже в игре, когда заходит
    public class InGameHandler : AuthorizationHandler<NotInGameRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        public InGameHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, NotInGameRequirement requirement)
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<GameHub>>();
            var username = context.User.Identity?.Name;
            HubCallerContext playerInQueue = GameHub.playersInQueue.FirstOrDefault(p => p.User.Identity.Name == username, null);
            Game playerInGame = GameHub.playersInGame.Values.FirstOrDefault(g => g.X.User.Identity.Name == username || g.O.User.Identity.Name == username, null);

            if (username != null && (playerInQueue != null || playerInGame != null))
            {
                bool result = false;
                HubCallerContext player = null;

                if (playerInGame != null)
                {
                    if (playerInGame.X.User.Identity.Name == username)
                        player = playerInGame.X;
                    else
                        player = playerInGame.O;
                }
                else
                    player = playerInQueue;

                result = await GameHub.CheckConnection(hubContext, player);
                if (result) // есть соединение, этого типа можно отсеять
                {
                    context.Fail();
                }
                else // нет соединения, подменить
                {
                    if (player == playerInQueue)
                    {
                        GameHub.playersInQueue.Remove(player);
                        context.Succeed(requirement);
                    }
                    else
                    {
                        GameHub.reconnectionList.Add(username);
                        context.Succeed(requirement);

                    }
                }
            }
            else
            {
                context.Succeed(requirement);
            }
            return;
        }
    }
}
