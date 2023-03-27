using AngularAuthAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace AngularAuthAPI.Helpers
{
    public static class Utilities
    {
        public static string GenerateToken(string key, string issuer, string audience, ApplicationUser user, ICollection<string> roles)
        {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtToken = new JwtSecurityToken(issuer, audience,
               claims,
               expires: DateTime.Now.AddMinutes(30),
               signingCredentials: credentials);

            String token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return token;

        }

        public static bool IsValidEmailAddress(string email)
        {
            try
            {
                MailAddress mailAddress = new MailAddress(email);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
