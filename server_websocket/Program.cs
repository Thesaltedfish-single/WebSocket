using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static light.http.server.httpserver;

namespace server_websocket {

    class Program
    {
        static void Main(string[] args)
        {
            var server = new light.http.server.httpserver();
            server.SetFailAction(on404);
            server.SetWebsocketAction("/ws", onWebSocketConnect);
            server.Start(1988);
            Console.WriteLine("服务器在端口1988打开");
            while (true)
            {
                Console.ReadLine();
            }
            static async Task on404(HttpContext context)
            {
                await context.Response.WriteAsync("获得了404");
            }
            static IWebSocketPeer onWebSocketConnect(System.Net.WebSockets.WebSocket websocket)
            {
                return new MyPeer(websocket);//返回一个MyPeer实例
            }
        }
    }


    class MyPeer:IWebSocketPeer
    {
        System.Net.WebSockets.WebSocket websocket;
        public MyPeer(System.Net.WebSockets.WebSocket websockets)
        {
            this.websocket = websocket;
        }

        public async Task OnConnect()
        {
            Console.WriteLine("连接");
            return;
        }
        public async Task OnDisConnect()
        {
            Console.WriteLine("断开连接");
            return;
        }

        public async Task OnRecv(MemoryStream stream,int count)
        {
            byte[] buf = new byte[count];
            stream.Read(buf, 0, buf.Length);//第二个参数为漂移量
            var str = System.Text.Encoding.UTF8.GetString(buf);//把接收的数据流转换为string

            Console.WriteLine("接收到:" + str);

            var outbuf = System.Text.Encoding.UTF8.GetBytes("你好");

            await SendAsync(outbuf);//回复

        }

        private async Task SendAsync(byte[] msg)
        {
            await this.websocket.SendAsync(
                msg,
                System.Net.WebSockets.WebSocketMessageType.Text,
                true,
               CancellationToken.None
               );
        }


    }



}
