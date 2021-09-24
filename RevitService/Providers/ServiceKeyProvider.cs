using System;

namespace RevitService.Providers
{
    public class ServiceKeyProvider : IServiceKeyProvider
    {
        public string Next()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
