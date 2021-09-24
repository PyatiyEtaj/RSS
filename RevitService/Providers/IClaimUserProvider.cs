using RevitService.Models;
using System.Security.Claims;

namespace RevitService.Providers
{
    public interface IClaimUserProvider
    {
        public User From(ClaimsPrincipal claims);
    }
}
