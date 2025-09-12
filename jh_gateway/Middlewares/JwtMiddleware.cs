using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace jh_gateway.Middlewares
{
    /// <summary>
    /// 
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration; // Inject IConfiguration to access settings

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="configuration"></param>
        public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token == null)
            {
                if (!ValidateRoute(context))
                {
                    throw new UnauthorizedAccessException("You are not authorize to access this url.");
                }

                await _next(context);
            }
            else
            {
                await AttachUserToContext(context, token);
            }

            await _next(context);
        }

        private bool ValidateRoute(HttpContext context)
        {
            if (context.Request.Path.ToString().ToLower().Contains("/login") ||
                context.Request.Path.ToString().ToLower().Contains("/registration"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!); // Get secret key from configuration

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
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception("Token expired/invalid.", ex);
                // Token validation failed, do nothing or log the error
            }
        }
    }
}
