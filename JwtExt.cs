using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MovieToHLS;

public static class JwtExt
{
    public static string CreateToken(this Guid userId)
    {
        var now = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim("user_id", userId.ToString())
        };

        var jwt = new JwtSecurityToken(
            claims: claims,
            expires: now.Add(TimeSpan.FromHours(24)),
            signingCredentials: new SigningCredentials(SecretKey, SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        return encodedJwt;
    }

    public static Guid UserId(this ClaimsPrincipal x) => Guid.Parse(x.FindFirst("user_id")!.Value);



    public static SymmetricSecurityKey SecretKey = new("asdkaskjbshdjgkbvsdghfvskjdfkhsbdf"u8.ToArray());


}