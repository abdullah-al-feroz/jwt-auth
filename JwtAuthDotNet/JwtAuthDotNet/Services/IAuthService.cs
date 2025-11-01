using JwtAuthDotNet.Dtos.Request;
using JwtAuthDotNet.Dtos.Response;

namespace JwtAuthDotNet.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(UserRegisterDto request);
        Task<TokenResponseDto> LoginAsync(UserLoginDto request);
        Task<TokenResponseDto> RefreshTokensAsync(RefreshTokenRequestDto request);

        //reset password
        Task<bool> SendResetPasswordEmailAsync(ForgotPasswordDto dto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
        Task<bool> LogoutAsync(LogoutRequestDto dto);
    }
}
