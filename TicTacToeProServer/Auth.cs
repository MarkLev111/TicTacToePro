using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicTacToePro.Shared;
using Microsoft.AspNetCore.SignalR;

namespace TicTacToeProServer
{
    [ApiController]
    [Route("api/auth")]
    public class Auth : ControllerBase
    {
        private readonly DBContext dbContext; // SQL
        private readonly IConfiguration configuration; // ради JWT чтобы не хардкодить ключ
        private readonly ILogger<Auth> logger; // всё пишем в логи ради азура

        public Auth(DBContext dbContext, IConfiguration configuration, ILogger<Auth> logger)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpPost("login")] // Полный путь: /api/auth/login
        public async Task<IActionResult> Authorize([FromBody] UserData data) // запрос на логин или регистрацию
        {
            if (data.email == null) // логин
            {
                logger.LogInformation($"Попытка входа в аккаунт {data.username}");
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.username == data.username);
                if (user == null)
                    return Unauthorized("Такого пользователя не существует.");
                else
                {
                    bool passwordCheck = BCrypt.Net.BCrypt.Verify(data.password, user.password);
                    if (!passwordCheck)
                        return Unauthorized("Неверный пароль.");
                }
                data.email = user.email;
            }
            else // регистрация
            {
                logger.LogInformation($"Попытка входа в аккаунт {data.username}");
                data.password = BCrypt.Net.BCrypt.HashPassword(data.password);

                bool existsEmail = await dbContext.Users.AnyAsync(u => u.email == data.email);
                bool existsUsername = await dbContext.Users.AnyAsync(u => u.username == data.username);
                if (existsEmail && existsUsername)
                    return Unauthorized("Игрок с такой почтой и таким никнеймом уже существует.");
                else if (existsEmail)
                    return Unauthorized("Игрок с такой почтой уже существует.");
                else if (existsUsername)
                    return Unauthorized("Игрок с таким никнеймом уже существует.");
                else
                {
                    dbContext.Users.Add(data);
                    await dbContext.SaveChangesAsync();
                }
            }

            string token = SetToken(data);
            return Ok(token);
        }

        private string SetToken(UserData user)
        {
            var claims = new List<Claim> // общедоступная инфа о токене
            {
                new Claim(ClaimTypes.Name, user.username),
                new Claim(ClaimTypes.Email, user.email),
                new Claim("RegistrationDate", DateTime.UtcNow.ToString()) // а надо ли?
            };

            // 2. Достаем секретный ключ из конфига
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
