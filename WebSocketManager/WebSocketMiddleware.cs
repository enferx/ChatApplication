using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebSocketManager;

namespace WebSocketManager
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private WebSocketHandler _handler;

        public WebSocketMiddleware(RequestDelegate next, WebSocketHandler handler)
        {
            _next = next;
            _handler = handler;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }
            CancellationToken ct = context.RequestAborted;
            WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
            var socketId = Guid.NewGuid().ToString();

            await _handler.ConnectAsync(socketId, currentSocket, ct);

            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                var response = await _handler.ReceiveMessageAsync(currentSocket, ct);
                if(string.IsNullOrEmpty(response))
                {
                    if(currentSocket.State != WebSocketState.Open)
                    {
                        break;
                    }

                    continue;
                }

                foreach (var socket in _handler.GetAll())
                {
                    if(socket.State != WebSocketState.Open)
                    {
                        continue;
                    }

                    await _handler.SendMessageAsync(socket, response, ct);
                }
            }

            await _handler.DisconnectAsync(socketId, ct);       
        }
    }
}
