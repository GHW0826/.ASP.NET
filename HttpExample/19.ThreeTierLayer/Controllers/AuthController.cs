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
                return BadRequest(new { message = "이미 존재하는 사용자입니다." });

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

            return Ok(new { message = "회원가입 완료" });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "사용자를 찾을 수 없습니다." });

            // 현재 사용자와 요청된 ID가 다르면 거부 (또는 Admin 검사)
            var currentUsername = User.Identity.Name;
            if (user.Username != currentUsername && !User.IsInRole("Admin"))
                return Forbid();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "사용자 삭제 완료" });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || PasswordHasher.Verify(request.Password, user.Password) == false)
            {
                return Unauthorized(new ErrorResponse
                {
                    Message = "아이디 또는 비밀번호가 올바르지 않습니다.",
                    Code = "InvalidCredentials"
                });
            }

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7); // 7일 유효
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
                new Claim(ClaimTypes.Role, role), // Role Claim (표준 ClaimTypes.Role 사용)
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
                return Unauthorized(new { message = "Refresh Token이 유효하지 않거나 만료되었습니다." });

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
