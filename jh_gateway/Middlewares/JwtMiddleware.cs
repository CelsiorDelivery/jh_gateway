using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace jh_gateway.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration; // Inject IConfiguration to access settings

        public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await AttachUserToContext(context, token);
            }

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]); // Get secret key from configuration

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // Set to true and configure if you have an issuer
                    ValidateAudience = false, // Set to true and configure if you have an audience
                    ClockSkew = TimeSpan.Zero // No tolerance for expiration time
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                // You can extract claims from jwtToken.Claims here and populate user context
                // For example, context.Items["User"] = new User { Id = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value) };
            }
            catch
            {
                // Token validation failed, do nothing or log the error
            }
        }
    }
}
