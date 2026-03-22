using Microsoft.AspNetCore.SignalR;
using TicTacToeProServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

// Указываем путь, по которому WPF будет подключаться
// Например: https://localhost:7001/game
app.MapHub<GameHub>("/gamehub");

Console.WriteLine("Сервер запущен");

app.Run();