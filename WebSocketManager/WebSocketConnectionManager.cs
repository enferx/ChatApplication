using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketManager
{
    public class WebSocketConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public WebSocket GetById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key == id).Value;
        }

        public string GetIdBySocket(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        public IEnumerable<WebSocket> GetAll()
        {
            return _sockets.Values;
        }

        public void AddSocket(string id, WebSocket socket)
        {
            _sockets.TryAdd(id, socket);
        }

        public async Task RemoveSocketAsync(string id, CancellationToken ct = default(CancellationToken))
        {
            //var id = _sockets.FirstOrDefault(p => p.Value == socket).Key;
            _sockets.TryRemove(id, out var removedSocket);
            if (removedSocket.State != WebSocketState.Open) 
                return;
            await removedSocket.CloseAsync
            (
                closeStatus: WebSocketCloseStatus.NormalClosure,
                statusDescription: "Closed by the WebSocketManager",
                cancellationToken: ct
            );
            removedSocket.Dispose();
        }

        
    }
}
