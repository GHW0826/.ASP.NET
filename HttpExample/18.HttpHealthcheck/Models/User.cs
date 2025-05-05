namespace HttpHealthcheck.Models
{
    public class User
    {
        public int Id { get; set; }  // PK
        public string Username { get; set; }
        public string Password { get; set; } // 단순 예제, 해싱 필요
        public string Role { get; set; } //  "Admin", "User"

        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpireTime { get; set; }
    }
}
