using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using ThreeTierLayer.Data;
using System.Security.Cryptography;
using ThreeTierLayer.Helpers;
using ThreeTierLayer.Models;
using Microsoft.AspNetCore.Authorization;

namespace ThreeTierLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { message = "�̹� �����ϴ� ������Դϴ�." });

            var user = new User
            {
                Username = request.Username,
                Password = PasswordHasher.Hash(request.Password),
                Role = request.Role,
                RefreshToken = "",
                RefreshTokenExpireTime = DateTime.MinValue
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ȸ������ �Ϸ�" });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "����ڸ� ã�� �� �����ϴ�." });

            // ���� ����ڿ� ��û�� ID�� �ٸ��� �ź� (�Ǵ� Admin �˻�)
            var currentUsername = User.Identity.Name;
            if (user.Username != currentUsername && !User.IsInRole("Admin"))
                return Forbid();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "����� ���� �Ϸ�" });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || PasswordHasher.Verify(request.Password, user.Password) == false)
            {
                return Unauthorized(new ErrorResponse
                {
                    Message = "���̵� �Ǵ� ��й�ȣ�� �ùٸ��� �ʽ��ϴ�.",
                    Code = "InvalidCredentials"
                });
            }

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7); // 7�� ��ȿ
            await _context.SaveChangesAsync();
            return Ok(new
            {
                accessToken = GenerateJwtToken(user.Username, user.Role),
                refreshToken = user.RefreshToken
            });
        }

        private string GenerateJwtToken(string username, string role)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(ClaimTypes.Role, role), // Role Claim (ǥ�� ClaimTypes.Role ���)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
            if (user == null || user.RefreshTokenExpireTime < DateTime.UtcNow)
                return Unauthorized(new { message = "Refresh Token�� ��ȿ���� �ʰų� ����Ǿ����ϴ�." });

            var newAccessToken = GenerateJwtToken(user.Username, user.Role);
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class RefreshRequest
        {
            public string RefreshToken { get; set; }
        }

    }
}
