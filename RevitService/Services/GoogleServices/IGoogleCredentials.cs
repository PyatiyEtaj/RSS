using Google.Apis.Auth.OAuth2;

namespace RevitService.Services.GoogleServices
{
    public interface IGoogleCredentials
    {
        GoogleCredential Credential { get; }
    }
}
