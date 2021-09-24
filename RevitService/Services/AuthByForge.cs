using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RevitService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Timers;

namespace RevitService.Services
{
    public class AuthByForgeOpts : AuthenticationSchemeOptions { }
    public class AuthByForge : AuthenticationHandler<AuthByForgeOpts>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // импровизированный кеш, так как ходить в автодеск очень долго
        // спасибо папаша за быстрое соединение
        private static readonly ConcurrentDictionary<string, (DateTime elapsedAt, AuthenticationTicket ticket)> _users;
        static AuthByForge()
        {
            _users = new();
            Timer timer = new(10 * 60 * 1000); // 10 минут
            timer.AutoReset = true;
            timer.Elapsed += (_, _) =>
            {
                foreach (var v in _users)
                {
                    if (v.Value.elapsedAt < DateTime.Now)
                    {
                        _users.TryRemove(v.Key, out var _);
                    }
                }
            };
            timer.Start();
        }

        public AuthByForge(
            IOptionsMonitor<AuthByForgeOpts> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IHttpClientFactory httpClientFactory
        ) : base(options, logger, encoder, clock)
            => _httpClientFactory = httpClientFactory;

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                string authorization = Request.Headers["Authorization"];
                if (authorization is not null)
                {
                    var item = _users.GetValueOrDefault(authorization);
                    if (item != default)
                    {                        
                        return AuthenticateResult.Success(item.ticket);
                    }

                    var client = _httpClientFactory.CreateClient();
                    var req = new HttpRequestMessage(
                        HttpMethod.Get, @"https://developer.api.autodesk.com/userprofile/v1/users/@me");
                    req.Headers.Add("Authorization", authorization);
                    var response = await client.SendAsync(req);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var user = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
                        ClaimsPrincipal principal = new(
                            new ClaimsIdentity(new Claim[] {
                                new Claim("UserName", user.UserName),
                                new Claim("EmailId", user.EmailId),
                                new Claim("UserId", user.UserId),
                            }, Scheme.Name)
                        );
                        var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);
                        _users.TryAdd(authorization, (DateTime.Now.AddHours(1), ticket));
                        return AuthenticateResult.Success(ticket);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during auth {ex.Message}");
            }
            return AuthenticateResult.NoResult();
        }
    }
}
