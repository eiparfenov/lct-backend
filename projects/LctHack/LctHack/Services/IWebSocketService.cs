using System.Net.WebSockets;
using System.Text;
using LctHack.Models;

namespace LctHack.Services;

public interface IWebSocketService
{
    Task MainLoop(string formTitle, WebSocket socket);
    Task NotifyStateChanged(string formTitle, VideoState state);
}

public class WebSocketService(ILogger<IWebSocketService> logger) : IWebSocketService
{
    private readonly Dictionary<string, WebSocket> _sockets = new();
    public async Task MainLoop(string formTitle, WebSocket socket)
    {
        _sockets[formTitle] = socket;
        logger.LogInformation("Socket connected for file {FormTitle}", formTitle);
        var buffer = new byte[1024 * 4];
        var receiveResult = await socket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            receiveResult = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await socket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    public async Task NotifyStateChanged(string formTitle, VideoState state)
    {
        logger.LogInformation("State of file {FormTitle} changed to {VideoState}", formTitle, state);
        if (_sockets.TryGetValue(formTitle, out var socket))
        {
            await socket.SendAsync(Encoding.UTF8.GetBytes(((int)state).ToString()), WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
        }
    }
}