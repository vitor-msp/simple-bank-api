using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Infra;

public class TokenProvider : ITokenProvider
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly TokenConfiguration _configuration;
    private readonly byte[] _key;

    public TokenProvider(IOptions<TokenConfiguration> options)
    {
        _configuration = options.Value;
        _key = Encoding.UTF8.GetBytes(_configuration.Key ?? "");
    }

    public string Generate(IAccount account)
    {
        var credentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature);
        var descriptor = new SecurityTokenDescriptor()
        {
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddSeconds(_configuration.AccessTokenExpiresInSeconds),
            Subject = GenerateClaims(account)
        };
        var token = _tokenHandler.CreateToken(descriptor);
        return _tokenHandler.WriteToken(token);
    }

    private static ClaimsIdentity GenerateClaims(IAccount account)
    {
        var ci = new ClaimsIdentity();
        ci.AddClaim(new Claim(ClaimTypes.Name, account.GetFields().AccountNumber.ToString()));
        return ci;
    }
}