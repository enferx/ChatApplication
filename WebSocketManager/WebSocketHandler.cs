using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketManager
{
    public class WebSocketHandler
    {
        private WebSocketConnectionManager _connectionManager;

        public WebSocketHandler(WebSocketConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public IEnumerable<WebSocket> GetAll()
        {
            return _connectionManager.GetAll();
        }

        public string GetIdBySocket(WebSocket socket)
        {
            return _connectionManager.GetIdBySocket(socket);
        }

        public async Task ConnectAsync(string id, WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            _connectionManager.AddSocket(id, socket);
            await SendMessageAsync(socket, "Connected", ct).ConfigureAwait(false);
        }

        public async Task DisconnectAsync(string id, CancellationToken ct= default(CancellationToken))
        {
            await _connectionManager.RemoveSocketAsync(id, ct);
        }

        public virtual async Task SendMessageAsync(WebSocket socket, string message, CancellationToken ct = default(CancellationToken))
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);
            await socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }

        public async Task<string> ReceiveMessageAsync(WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();
                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);
                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }
                // Encoding UTF8: https://tools.ietf.org/html/rfc6455#section-5.6
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
