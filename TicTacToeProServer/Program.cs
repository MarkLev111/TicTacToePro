using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TicTacToeProServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("H76?7w6eh7HGE!23w6h7&6@6pWt7@6yw87t"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // используем JWT
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters // параметры, по которым будет проверяться пользовательский токен АВТОМАТОМ !!!
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,

        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // убираем погрешность
    };

    options.Events = new JwtBearerEvents // обработка получения токена от пользователя
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // подключение к хабу -> токен часть пользователя
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/gamehub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true) // все могут обращаться к серверу
              .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("UsersDB");

builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<GameHub>("/gamehub");

Console.WriteLine("Сервер запущен");

app.Run();