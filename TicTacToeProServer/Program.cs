using Microsoft.AspNetCore.SignalR;
using TicTacToeProServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true) // все могут обращаться к серверу
              .AllowCredentials();
    });
});

var app = builder.Build();

app.MapHub<GameHub>("/gamehub");

app.UseCors();

Console.WriteLine("Сервер запущен");

app.Run();