using Remp.Models.Entities;
using Remp.Models.Responses;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Remp.API.Services.Auth;

public class JwtTokenService: IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public LoginResponse GenerateToken(ApplicationUser user, IList<string> roles)
    {
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        List<Claim> claims = [];
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
        foreach(string role in roles)
        {
          claims.Add(new Claim(ClaimTypes.Role, role));
        }
        byte[] keyBytes = Convert.FromBase64String(_jwtSettings.SecretKey);
        SymmetricSecurityKey key = new SymmetricSecurityKey(keyBytes);
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(issuer: _jwtSettings.Issuer, audience: _jwtSettings.Audience, claims: claims, expires: expiresAt, signingCredentials: credentials);
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        string tokenString = handler.WriteToken(token);

        LoginResponse response = new LoginResponse{
          Token = tokenString, 
          ExpiresAt = expiresAt, 
          UserId = user.Id, 
          Email = user.Email ?? "", 
          Roles = roles.ToList()
        };
    
        return response;
    
    }
}