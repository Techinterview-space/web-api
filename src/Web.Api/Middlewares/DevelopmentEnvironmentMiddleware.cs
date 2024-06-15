using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Web.Api.Middlewares
{
    public class DevelopmentEnvironmentMiddleware
    {
        private readonly RequestDelegate _next;

        public DevelopmentEnvironmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "admin");
            var userNameClaim = new Claim(ClaimTypes.Name, "admin");
            var givenNameClaim = new Claim(ClaimTypes.GivenName, "admin");
            var surNameClaim = new Claim(ClaimTypes.Surname, "admin");
            var emailClaim = new Claim(ClaimTypes.Email, "admin@test.com");
            var emailVerifiedClaim = new Claim("email_verified", "true");
            var roleClaims = new List<Claim>();
            foreach (Role role in Enum.GetValues(typeof(Role)))
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var allClaims = new List<Claim>
            {
                userIdClaim,
                userNameClaim,
                emailClaim,
                givenNameClaim,
                surNameClaim,
                emailVerifiedClaim
            };
            allClaims.AddRange(roleClaims);

            var identity = new ClaimsIdentity(allClaims, "TestAuthentication");
            var userPrincipal = new ClaimsPrincipal(identity);
            context.User = userPrincipal;

            await _next(context);
        }
    }
}
