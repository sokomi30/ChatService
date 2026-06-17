using System.Text;
using ChatService.Api.Models;
using ChatService.Api.WebSockets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
var mongoConnection = builder.Configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));
builder.Services.AddScoped<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("chatdb"));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoDatabase>().GetCollection<ChatMessage>("messages"));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoDatabase>().GetCollection<User>("users"));
builder.Services.AddScoped<ChatWebSocketHandler>();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ChatServiceSecretKey1234567890123456!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ChatService",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ChatClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddCors();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); // ← до UseAuthentication
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
app.MapControllers();

app.Map("/ws", async (HttpContext context, ChatWebSocketHandler handler) =>
{
    var username = context.Request.Query["username"].ToString();
    if (string.IsNullOrEmpty(username))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Username required");
        return;
    }

    if (context.WebSockets.IsWebSocketRequest)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        await handler.HandleAsync(socket, username);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();