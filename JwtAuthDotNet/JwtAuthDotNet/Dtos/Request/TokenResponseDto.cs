using JwtAuthDotNet.Dtos.Response;

namespace JwtAuthDotNet.Dtos.Request
{
    public class TokenResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public virtual UserResponseDto User {  get; set; }
    }
}
