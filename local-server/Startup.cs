using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace local_server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var rootDir = FindProjRootDir(new DirectoryInfo(Directory.GetCurrentDirectory()));
            if (rootDir is null)
            {
                throw new InvalidOperationException($"Cannot find proj root dir from '{Directory.GetCurrentDirectory()}'");
            }
            var staticPath = Path.Combine(rootDir, "docs");

            logger.LogInformation($"Serving static files from '{staticPath}'");

            var options = new RewriteOptions();
            options.Add(ctx =>
            {
                var request = ctx.HttpContext.Request;
                if (!request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var fileName = request.Path.Value.Substring(request.Path.Value.LastIndexOf('/') + 1);
                var dotIndex = fileName.LastIndexOf('.');
                if (dotIndex >= 0)
                {
                    return;
                }

                if (fileName == string.Empty)
                {
                    request.Path = new Microsoft.AspNetCore.Http.PathString("/index.html");
                }
                else
                {
                    request.Path = new Microsoft.AspNetCore.Http.PathString($"{request.Path.Value}.html");
                }

            });

            app.UseRewriter(options);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(staticPath),
                RequestPath = "",
                ServeUnknownFileTypes = true,
                DefaultContentType = "text/html"
            });
        }

        public string FindProjRootDir(DirectoryInfo dir)
        {
            if (dir.GetFiles().Any(f => f.Name == "config.wyam"))
            {
                return dir.FullName;
            }

            return dir.Parent is null ? null : FindProjRootDir(dir.Parent);
        }
    }
}
