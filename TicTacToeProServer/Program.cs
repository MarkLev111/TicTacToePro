using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
    options.TokenValidationParameters = new TokenValidationParameters // параметры, по которым будет проверяться пользовательский токен
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,

        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // убираем погрешность
    };

    options.Events = new JwtBearerEvents // обработка получения токена от пользователя
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Если запрос идет к хабу, подкладываем токен в систему проверки
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

var app = builder.Build();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<GameHub>("/gamehub");

Console.WriteLine("Сервер запущен");

app.Run();