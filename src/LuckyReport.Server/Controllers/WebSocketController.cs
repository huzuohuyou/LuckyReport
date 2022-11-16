using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketsSample.Controllers;

// <snippet>
public class WebSocketController : ControllerBase
{
    private Dictionary<string, WebSocket> _WebSockets = new Dictionary<string, WebSocket>();
    [HttpGet("/{name}")]
    public async Task Get([FromRoute][Required] string name)
    {
        
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            if(_WebSockets.ContainsKey(name)) throw new Exception("名称重复");
            _WebSockets.Add(name, webSocket);
            
            await Echo(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    // </snippet>
    protected static ArraySegment<byte> GetBuffer(byte[] arr)
    {
        return new ArraySegment<byte>(arr);
    }
    protected static ArraySegment<byte> GetBuffer(string str)
    {
        return GetBuffer(Encoding.UTF8.GetBytes(str));
    }
    protected static string m_result = null;
    private static async Task Echo(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 400];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receiveResult = await webSocket.ReceiveAsync(
                GetBuffer(buffer), CancellationToken.None);
            m_result += Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}
