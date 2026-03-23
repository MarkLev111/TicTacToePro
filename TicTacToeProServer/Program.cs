using Microsoft.AspNetCore.SignalR;
using TicTacToeProServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<GameHub>("/gamehub");

Console.WriteLine("Сервер запущен");

app.Run();