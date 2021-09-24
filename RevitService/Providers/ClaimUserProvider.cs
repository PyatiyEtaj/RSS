using RevitService.Models;
using System.Security.Claims;

namespace RevitService.Providers
{
    public class ClaimUserProvider : IClaimUserProvider
    {
        public User From(ClaimsPrincipal claims)
        {
            return new User { 
                EmailId = claims.FindFirstValue("EmailId"),
                UserId = claims.FindFirstValue("UserId"),
                UserName = claims.FindFirstValue("UserName")
            };
        }
    }
}
