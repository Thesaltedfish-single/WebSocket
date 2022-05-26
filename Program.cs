using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace testdot
{
    class Program
    {
        static IWebHost host;
        static void Main(string[] args)
        {
            var server = new light.http.server.httpserver();//light?,������û�У���ʾ��light����using System.Windows.Media.Media3D;��
            server.SetFailAction(on404);
            server.SetHttpAction("/abc", onAbc);
            server.SetHttpAction("/aaa", onAaa);
            server.SetHttpController("/con_abc", new MyController());//����pathȥ��·�ɱ�
            server.Start(1988);
            Console.WriteLine("server on:1988");
            while (true)
            {
                Console.ReadLine();
            }
        }

        //��Ӧ����
        static async Task on404(HttpContext context)
        {
            await context.Response.WriteAsync("you got a 404");
        }
        static async Task onAbc(HttpContext context)
        {
            await context.Response.WriteAsync("you got a abc");

        }
        static async Task onAaa(HttpContext context)
        {
            await context.Response.WriteAsync("you got a aaa");
        }
    }
    class MyController : light.http.server.IController
    {
        public async Task ProcessAsync(HttpContext context, string path)
        {
            await context.Response.WriteAsync("you got a MyController");
        }
    }


    //�������Ӧ���Ƿ���ʲôλ��
    public void Star(int port, int potForHttps = 0, string pfxpath = null, string password = null)
    {
        host = new WebHostBuilder().UseKestrel((options) =>//host��ʲô
        {
            options.Listen(IPAddress.Any, port, listenOptions =>
            {

            });
            if (portForHttps != 0)
            {
                options.Listen(IPAddress.Any, portForHttps, listenOptions =>
                {
                    listenOptions.UseHttps(pfxpath, password);
                });
            }
        }).Configure(app =>  //����socket
        {
            app.UseWebSockets();
            app.UseResponseCompression();
            app.Run(ProcessAsync);  //�첽����
        }).ConfigureServices(services =>
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = false;
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" })//ͼƬ����û�и�ȫ


                });
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

        }).Build();


        host.Start();


    }
}