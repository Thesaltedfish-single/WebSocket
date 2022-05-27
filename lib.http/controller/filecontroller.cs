using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace light.http.server
{
    /// <summary>
    /// 对应一个文件夹提供www服务
    /// </summary>
    public class filecontroller : IController
    {
        string localpath;
        string defaultfile;
        Dictionary<string, string> txtmime = new Dictionary<string, string>();
        Dictionary<string, string> binmime = new Dictionary<string, string>();
        public filecontroller(string localpath, string defaultfile = "index.html")
        {
            this.localpath = localpath;
            this.defaultfile = defaultfile;
            txtmime[".txt"] = "text/plain;charset=UTF-8";
            txtmime[".js"] = "text/plain;charset=UTF-8";
            txtmime[".ts"] = "text/plain;charset=UTF-8";
            txtmime[".js.map"] = "text/plain;charset=UTF-8";
            txtmime[".map"] = "text/plain;charset=UTF-8";
            txtmime[".json"] = "text/plain;charset=UTF-8";
            txtmime[".xml"] = "text/plain;charset=UTF-8";
            txtmime[".html"] = "text/html;charset=UTF-8";
            txtmime[".htm"] = "text/html;charset=UTF-8";

            binmime[".bin"] = "application/octet-stream";
            binmime[".jpg"] = "image/jpeg";
            binmime[".jpeg"] = "image/jpeg";
            binmime[".png"] = "image/png";
            binmime[".gif"] = "image/gif";
        }
        public void SetTxtMime(string ext,string contenttype)
        {
            txtmime[ext] = contenttype;
        }
        public void SetBinMime(string ext, string contenttype)
        {
            binmime[ext] = contenttype;
        }
        public async Task SendFile(HttpContext context, string filename)
        {
            //这里没处理分块请求，因为暂时面对网页展示，还没有大文件和流式数据
            var ext = System.IO.Path.GetExtension(filename);
            if (txtmime.ContainsKey(ext))
            {
                var txt = System.IO.File.ReadAllText(filename);
                context.Response.ContentType = txtmime[ext];
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(txt);
                return;
            }
            else
            {
                if (binmime.ContainsKey(ext))
                {
                    context.Response.ContentType = binmime[ext];
                }
                else
                {
                    context.Response.ContentType = "application/octet-stream";
                }
                context.Response.StatusCode = 200;
                await context.Response.StartAsync();
                var bin = System.IO.File.ReadAllBytes(filename);
                await context.Response.Body.WriteAsync(bin, 0, bin.Length);
            }
        }
        public async Task ProcessAsync(HttpContext context, string path)
        {
            try
            {

                if (context.Request.Method != "GET")
                {
                    context.Response.ContentType = "text/plain;charset=UTF-8";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("only support get:" + context.Request.Path.Value);
                    return;
                }
                if (path == "")
                    path = defaultfile;
                var filename = System.IO.Path.Combine(localpath, path.Replace('!', System.IO.Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filename))
                {
                    await SendFile(context, filename);
                }
                else
                {
                    context.Response.ContentType = "text/plain;charset=UTF-8";
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("not found at:" + context.Request.Path.Value);
                    return;
                }
            }
            catch (Exception err)
            {
                try
                {
                    context.Response.ContentType = "text/plain;charset=UTF-8";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("error on got url:" + context.Request.Path.Value);
                }
                catch
                {

                }
            }
        }
    }
}
