namespace JwtAuthDotNet.Dtos.Response
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public virtual RoleResponseDto Role { get; set; }
    }
}
