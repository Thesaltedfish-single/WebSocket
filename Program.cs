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
            int port = 1988;
            host = new WebHostBuilder().UseKestrel((options) =>
            {
                options.Listen(IPAddress.Any, port, listenOptions => { });
            }).Configure(app =>
            {
                app.Run(ProcessAsync);
            }).Build();

            Console.WriteLine("http begin at:1988");
            host.Start();

            //mainthread loop
            while (true)
            {
                Console.ReadLine();
            }
        }
        static async Task ProcessAsync(HttpContext context)
        {
            try
            {
                context.Response.ContentType = "application/json;charset=UTF-8";
                context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST";
                context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
                context.Response.Headers["Access-Control-Max-Age"] = "31536000";
                var path = context.Request.Path.Value;
                await context.Response.WriteAsync("hello:" + path);
            }
            catch
            {

            }
        }
    }
}