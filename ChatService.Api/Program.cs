using System.Text;
using ChatService.Api.Middleware;
using ChatService.Api.Models;
using ChatService.Api.Services;
using ChatService.Api.WebSockets;
using Microsoft.AspNetCore.Authentication;
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

// Services (business logic layer)
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatMessageService, ChatMessageService>();
builder.Services.AddScoped<ChatWebSocketHandler>();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key is required. Set it via environment variables or configuration.");
}

var validIssuer = builder.Configuration["Jwt:Issuer"] ?? "ChatService";
var validAudience = builder.Configuration["Jwt:Audience"] ?? "ChatClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = validIssuer,
            ValidAudience = validAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ws"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins is { Length: > 0 })
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader();
            return;
        }

        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    // Include XML comments for documentation
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "ChatService.Api.xml");
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
});

// Logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Error handling middleware must be added early
app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatService API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
app.MapControllers();

app.Map("/ws", async (HttpContext context, ChatWebSocketHandler handler) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("WebSocket request expected");
        return;
    }

    var authResult = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
    if (!authResult.Succeeded)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    var username = context.User.Identity?.Name;
    if (string.IsNullOrWhiteSpace(username))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("User name claim is missing");
        return;
    }

    var socket = await context.WebSockets.AcceptWebSocketAsync();
    await handler.HandleAsync(socket, username);
}).RequireAuthorization();

app.Run();