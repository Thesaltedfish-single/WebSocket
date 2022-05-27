using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace light.http.server
{


    public class httpserver
    {
        public httpserver()
        {
            onHttpEvents = new System.Collections.Concurrent.ConcurrentDictionary<string, IController>();
        }
        private IWebHost host;
        public System.Collections.Concurrent.ConcurrentDictionary<string, IController> onHttpEvents;
        deleProcessHttp onHttp404;



        public void Start(int port, int portForHttps = 0, string pfxpath = null, string password = null)
        {
            host = new WebHostBuilder().UseKestrel((options) =>
            {
                options.Listen(IPAddress.Any, port, listenOptions =>
                  {

                  });
                if (portForHttps != 0)
                {
                    options.Listen(IPAddress.Any, portForHttps, listenOptions =>
                      {
                          //if (!string.IsNullOrEmpty(sslCert))
                          //if (useHttps)
                          listenOptions.UseHttps(pfxpath, password);
                          //sslCert, password);
                      });
                }
            }).Configure(app =>
            {

                app.UseWebSockets();
                app.UseResponseCompression();

                app.Run(ProcessAsync);
            }).ConfigureServices(services =>
            {
                services.AddResponseCompression(options =>
                {
                    options.EnableForHttps = false;
                    options.Providers.Add<GzipCompressionProvider>();
                    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
                });

                services.Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = CompressionLevel.Fastest;
                });
            }).Build();

            host.Start();
        }

        void RegEvent(string path,IController controller)
        {
            var paths = path.ToLower() .Split('/',StringSplitOptions.RemoveEmptyEntries);
            var tpath = "";
            foreach(var p in paths)
            {
                tpath += p + "!";
            }
            if (onHttpEvents.ContainsKey(path) == false)
            {
                onHttpEvents[tpath] = controller;
            }
        }
        IController GetEvent(string path,out string requestpath)
        {
            var paths = path.ToLower().Split('/', StringSplitOptions.RemoveEmptyEntries);
            var tpath = "";
            foreach (var p in paths)
            {
                tpath += p + "!";
            }
            foreach(var it in onHttpEvents)
            {
                if(tpath.IndexOf(it.Key)==0)
                {
                    requestpath = tpath.Substring(it.Key.Length);
                    return it.Value;
                }
            }
            requestpath = null;
            return null;
        }
        T TryRegEvent<T>(string path, System.Func<T> ctorfunc) where T  :class, IController
        {

            var e = GetEvent(path,out string requestpath) as T;
            if (e == null)
            {
                e = ctorfunc();
                RegEvent(path, e);
            }
            return e;
        }
        public void AddJsonRPC(string path, string method, JSONRPCController.ActionRPC action)
        {
            var jsonc = TryRegEvent(path, () => new JSONRPCController());
            jsonc.AddAction(method, action);
        }
        public void SetJsonRPCFail(string path, JSONRPCController.ActionRPCFail action)
        {
            var jsonc = TryRegEvent(path, () => new JSONRPCController());
            jsonc.SetFailAction(action);
        }
        public void SetHttpAction(string path, deleProcessHttp httpaction)
        {
            RegEvent(path, new ActionController(httpaction));
        }
        public void SetWebsocketAction(string path, deleWebSocketCreator websocketaction)
        {
            RegEvent(path, new WebSocketController(websocketaction));
        }
        public void SetHttpController(string path, IController controller)
        {
            RegEvent(path, controller);
        }
        public void SetFailAction(deleProcessHttp httpaction)
        {
            onHttp404 = httpaction;
        }
        public delegate Task deleProcessHttp(HttpContext context);
        public interface IWebSocketPeer
        {
            Task OnConnect();
            Task OnRecv(System.IO.MemoryStream stream, int count);
            Task OnDisConnect();
        }
        public delegate IWebSocketPeer deleWebSocketCreator(System.Net.WebSockets.WebSocket websocket);
        //public enum WebsocketEventType
        //{
        //    Connect,
        //    Disconnect,
        //    Recieve,
        //}
        //public delegate Task onProcessWebsocket(WebsocketEventType type, System.Net.WebSockets.WebSocket context, byte[] message = null);


        private async Task ProcessAsync(HttpContext context)
        {
            try
            {
                var path = context.Request.Path.Value;
                IController c = GetEvent(path,out string requestpath);
                if (c!=null)
                {
                    if(requestpath.Length>1)//超过一个字节，删除最后的标志
                    {
                        requestpath = requestpath.Substring(0, requestpath.Length - 1);
                    }
                    await c.ProcessAsync(context, requestpath);
                }
                else
                {
                    await onHttp404(context);
                }
            }
            catch
            {

            }
        }
    }
}
