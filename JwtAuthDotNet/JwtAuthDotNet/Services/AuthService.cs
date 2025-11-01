using AutoMapper;
using JwtAuthDotNet.Data;
using JwtAuthDotNet.Dtos.Request;
using JwtAuthDotNet.Dtos.Response;
using JwtAuthDotNet.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtAuthDotNet.Services
{
    public class AuthService(UserDbContext context, IConfiguration configuration, IMapper mapper) : IAuthService
    {
        public async Task<TokenResponseDto> LoginAsync(UserLoginDto request)
        {
            var user = await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user is null)
            {
                return null;
            }
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user),
                User = mapper.Map<UserResponseDto>(user)
            };
        }

        public async Task<UserResponseDto> RegisterAsync(UserRegisterDto request)
        {
            if (await context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return null;
            }

            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
            if (role is null)
                return null;

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = new PasswordHasher<User>().HashPassword(new User(), request.Password),
                Role = role
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return mapper.Map<UserResponseDto>(user);
        }

        public async Task<TokenResponseDto> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId.Value, request.RefreshToken);
            if (user is null)
                return null;

            return await CreateTokenResponse(user);
        }

        private async Task<User> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public async Task<bool> SendResetPasswordEmailAsync(ForgotPasswordDto dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user is null) return false;

            var token = GenerateSecureToken();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
            await context.SaveChangesAsync();

            //var resetLink = $"https://yourfrontend.com/reset-password?userId={user.Id}&token={token}";
            var resetLink = $"https://localhost:7069/swagger/index.html#/Auth/reset-password?token={token}";
            Console.WriteLine($"Password reset link: {resetLink}");
            var html = $"""
                        <p>Hello {user.Username},</p>
                        <p>Click <a href='{resetLink}'>here</a> to reset your password. This link expires in 30 minutes.</p> 
                        """;
            var emailService = new EmailService(configuration);
            await emailService.SendEmailAsync(user.Email, "Reset Your Password", html);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == dto.Token &&
                u.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (user is null) return false;

            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await context.SaveChangesAsync();
            return true;
        }

        private string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        public async Task<bool> LogoutAsync(LogoutRequestDto dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user is null || user.RefreshToken is null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await context.SaveChangesAsync();
            return true;
        }
    }
}
