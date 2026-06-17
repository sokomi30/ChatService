using ChatService.Api.WebSockets;
using MongoDB.Driver;
using System.Reflection;
using ChatService.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient("mongodb://localhost:27017"));
builder.Services.AddScoped<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("chatdb"));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoDatabase>().GetCollection<ChatMessage>("messages"));
builder.Services.AddScoped<ChatWebSocketHandler>();

var app = builder.Build();

app.UseWebSockets();

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