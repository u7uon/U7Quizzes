using CloudinaryDotNet;
using dotenv.net;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using U7Quizzes.AppData;
using U7Quizzes.Caching;
using U7Quizzes.DTOs;
using U7Quizzes.DTOs.Auth;
using U7Quizzes.DTOs.Share;
using U7Quizzes.Extensions;
using U7Quizzes.IRepository;
using U7Quizzes.IServices;
using U7Quizzes.IServices.Auth;
using U7Quizzes.Models;
using U7Quizzes.Repository;
using U7Quizzes.Services;
using U7Quizzes.SingalIR;
using U7Quizzes.Validators;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddLogging();
builder.Services.AddAutoMapper(typeof(Program));


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // mặc định challenge = Google
})
.AddCookie(

    ) // cần để lưu state và thông tin user
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Google:ClientSecret"];
    // KHÔNG đặt CallbackPath, để mặc định /signin-google
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var path = context.HttpContext.Request.Path;

            // ✅ Xử lý riêng cho SignalR
            if (path.StartsWithSegments("/quiz_session"))
            {
                Console.WriteLine("heeeeeeeeeeee");
                // SignalR: đọc từ query string trước, fallback sang cookie
                var accessToken = context.Request.Query["access_token"].ToString();
                Console.WriteLine(accessToken);
                context.Token = accessToken;
            }
            else
            {
                // REST API: đọc từ cookie
                context.Token = context.Request.Cookies["access_token"];
            }

            return Task.CompletedTask;
        }
    };
});






builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();


builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddSingleton<Cloudinary>(op =>
{
    var settings = op.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
    return new Cloudinary(account);

}
    );








//--- Cấu hình DI 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICachingService, CachingService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQuestionsRepository, QuestionRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<IResponseService, ResponseService>();
builder.Services.AddScoped<IResponseRepository, ResponseRepository>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();


//Validation
builder.Services.AddScoped<IValidator<RegisterDTO>, UserValidator>();


// Cấu hình db
builder.Services.AddDbContext<ApplicationDBContext>(options => options
                .UseSqlServer(builder.Configuration
                .GetConnectionString("Defaultconnection"))
    );


// cấu hình auth
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
}
).AddEntityFrameworkStores<ApplicationDBContext>()
.AddDefaultTokenProviders();


builder.Services.AddRazorPages();

//cấu hình caching
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("Redis")["ConnectionString"];
    options.InstanceName = "u7_redis_";
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// === SignalR ===



builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
});


var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

}
app.Use(async (context, next) =>
{

    var path = context.Request.Path;
    var token = context.Request.Query["access_token"].ToString();

    if (path.StartsWithSegments("/quiz_session") && !string.IsNullOrEmpty(token))
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            context.User = principal;
        }
        catch
        {
            context.Response.StatusCode = 401;
            return;
        }
    }

    await next();
});


app.UseHttpsRedirection();
app.UseCors("AllowLocalhost");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
app.UseResponseCaching();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
    endpoints.MapHub<QuizSessionHub>("/quiz_session");
});

app.Run();
