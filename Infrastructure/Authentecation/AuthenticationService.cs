using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs.Authentication;
using Application.Interfaces.Authentecation;
using AutoMapper;
using Domain.Entites.Enums;
using Domain.Entites.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentecation
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper mapper;
        private readonly ApplicationDbContext _context;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IRefreshTokenService _refreshTokenSerivce;
        public AuthenticationService(IConfiguration configuration, UserManager<ApplicationUser> userManager, IMapper mapper,
            ApplicationDbContext context, IEmailVerificationService emailVerificationService, IRefreshTokenService refreshTokenSerivce)
        {
            _configuration = configuration;
            _userManager = userManager;
            this.mapper = mapper;
            this._context = context;
            _emailVerificationService = emailVerificationService;
            _refreshTokenSerivce = refreshTokenSerivce;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Identity) ??
                    await _userManager.FindByNameAsync(request.Identity);

            if (user == null)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Invalid email, username, phone number, or password.",
                    Expiration = DateTime.UtcNow,
                    AccessToken = string.Empty
                };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Invalid email, username, phone number, or password.",
                    Expiration = DateTime.UtcNow,
                    AccessToken = string.Empty
                };
            }

            if (!user.EmailConfirmed)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Message = "Please confirm your email before logging in.",
                    Expiration = DateTime.UtcNow,
                    AccessToken = string.Empty
                };
            }

            var dto = mapper.Map<AuthResponse>(user);

            var refresh = await _refreshTokenSerivce.GenerateAsync(user.Id, ipAddress);

            await _context.SaveChangesAsync();

            dto.IsAuthenticated = true;
            dto.Message = "Login successful.";
            dto.AccessToken = await GenerateJwtToken(user);
            dto.Expiration = DateTime.UtcNow.AddMinutes(_configuration.GetValue<double>("Jwt:ExpiryMinutes"));
            dto.RefreshToken = refresh.RawToken;
            return dto;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(request.Email) ?? 
                await _userManager.FindByNameAsync(request.UserName);


                if (userExists != null)
                {
                    return new AuthResponse
                    {
                        IsAuthenticated = false,
                        Message = "Email, username, or phone number already exists.",
                        Expiration = DateTime.UtcNow,
                        AccessToken = string.Empty
                    };
                }

                var user = mapper.Map<ApplicationUser>(request);

                user.EmailConfirmed = false;

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return new AuthResponse
                    {
                        IsAuthenticated = false,
                        Message = string.Join(" | ", result.Errors.Select(e => e.Description)),
                        Expiration = DateTime.UtcNow,
                        AccessToken = string.Empty
                    };
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(user, Role.User.ToString());

                if (!addToRoleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return new AuthResponse
                    {
                        IsAuthenticated = false,
                        Message = string.Join(" | ", addToRoleResult.Errors.Select(e => e.Description))
                    };
                }
                await _emailVerificationService.SendEmailConfirmationAsync(user.Id);

                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Account created successfully. Please check your email to confirm your account before logging in.",
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber
                };
            }
            catch (Exception)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "An error occurred during registration."
                };
            }
        }

        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(userClaims);
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Key")!)
            );

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: _configuration.GetValue<string>("Jwt:Issuer"),
                audience: _configuration.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<double>("Jwt:ExpiryMinutes")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthResponse> RefreshAsync(string refreshToken, string? ipAddress)
        {
            try
            {
                var oldToken = await _refreshTokenSerivce.GetByRawTokenAsync(refreshToken);
                if (oldToken == null || !oldToken.IsActive)
                {
                    return new AuthResponse
                    {
                        IsAuthenticated = false,
                        Message = "Invalid refresh token.",
                        Expiration = DateTime.UtcNow,
                        AccessToken = string.Empty
                    };
                }
                var token = await _refreshTokenSerivce.RotateAsync(oldToken,
                oldToken.ApplicationUserId, ipAddress);

                var accessToken = await GenerateJwtToken(oldToken.ApplicationUser);
                await _context.SaveChangesAsync();
                return new AuthResponse
                {
                    IsAuthenticated = true,
                    Message = "Token refreshed successfully.",
                    Id = oldToken.ApplicationUserId,
                    Email = oldToken.ApplicationUser.Email,
                    UserName = oldToken.ApplicationUser.UserName,
                    AccessToken = accessToken,
                    Expiration = DateTime.UtcNow.AddMinutes(_configuration.GetValue<double>("Jwt:ExpiryMinutes")),
                    RefreshToken = token.RawToken
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Message = ex.Message
                };
            }
        }

        public async Task<bool> LogoutAsync(string refreshToken, string? ipAddress)
        {
            var token = await _refreshTokenSerivce.GetByRawTokenAsync(refreshToken);

            if (token == null || !token.IsActive)
                return false;

            var revoked = await _refreshTokenSerivce.RevokeAsync(token.ApplicationUserId, refreshToken, ipAddress);

            await _context.SaveChangesAsync();

            return revoked;
        }

        public async Task<bool> LogoutAllAsync(string userId, string? ipAddress)
        {
            var tokens = await _refreshTokenSerivce.GetByListTokenUserIdAsync(userId);
            if (!tokens!.Any())
                return false;

            foreach (var item in tokens!)
            {
                item.RevokedAt = DateTimeOffset.UtcNow;
                item.RevokedByIp = ipAddress;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}