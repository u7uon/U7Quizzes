using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace U7Quizzes.Attribute
{
    public class OptionalAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            string token = null;


            token = httpContext.Request.Cookies["access_token"];


            // Nếu có token, validate và set User
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
                    var tokenHandler = new JwtSecurityTokenHandler();

                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                    };

                    var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                    httpContext.User = principal;
                }
                catch
                {
                    // Token không hợp lệ, giữ User như cũ (anonymous)
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
