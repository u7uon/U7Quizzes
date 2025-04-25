using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using U7Quizzes.AppData;
using U7Quizzes.Models;
using U7Quizzes.SingalIR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



// Cấu hình db
builder.Services.AddDbContext<ApplicationDBContext>
    (options => options
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


//cấu hình caching
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "U7Quiz:";
});


// === SignalR ===
builder.Services.AddSignalR();


builder.Services.AddWebSockets(options => {
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
});




var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseWebSockets(); // WebSocket
app.UseAuthentication(); // Identity
app.UseAuthorization();

app.UseResponseCaching(); // Caching

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<U7Hub>("/quizHub"); // SignalR realtime endpoint
});

app.Run();
