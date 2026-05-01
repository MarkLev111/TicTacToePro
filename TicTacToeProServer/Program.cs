using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicTacToeProServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers();

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

        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // убираем погрешность
    };

    options.Events = new JwtBearerEvents // обработка получения токена от пользователя
    {
        //OnMessageReceived = context =>
        //{
        //    var accessToken = context.Request.Query["access_token"];

        //    // подключение к хабу -> токен часть пользователя
        //    var path = context.HttpContext.Request.Path;
        //    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/gamehub"))
        //    {
        //        context.Token = accessToken;
        //    }
        //    return Task.CompletedTask;
        //}

        OnMessageReceived = context =>
        {
            // 1. Пытаемся взять из Query (стандарт SignalR для сокетов)
            var accessToken = context.Request.Query["access_token"];

            // 2. Если в Query пусто, лезем в заголовки (стандарт для Negotiate/HTTP)
            if (string.IsNullOrEmpty(accessToken))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    accessToken = authHeader.Substring("Bearer ".Length).Trim();
                }
            }

            // 3. Если хоть где-то нашли — отдаем системе
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSingleton<IAuthorizationHandler, InGameHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyNewPlayers", policy =>
        policy.Requirements.Add(new NotInGameRequirement()));
});

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true) // все могут обращаться к серверу
              .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("UsersDB"); // БАЗА ДАННЫХ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    { // если база данных уснула, она будет долго пытаться подключиться
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

var app = builder.Build();

app.UseCors();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<GameHub>("/gamehub");

app.MapControllers();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value.ToLower();

    if (path.StartsWith("/login") || path.StartsWith("/css") || path.StartsWith("/js"))
    {
        await next();
        return;
    }

    if (context.Session.GetString("LoggedIn") != "true")
    {
        context.Response.Redirect("/Login");
        return;
    }

    await next();
});

app.MapRazorPages();

Console.WriteLine("Сервер запущен");

app.Run();
