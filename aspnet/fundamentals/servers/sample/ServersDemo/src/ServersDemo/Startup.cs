using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ServersDemo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public IConfigurationRoot Configuration { get; private set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

            app.UseStaticFiles();
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync($"Hosted by {Program.Server}\r\n\r\n");

                if (serverAddressesFeature != null)
                {
                    await context.Response.WriteAsync($"Listening on the following addresses: {string.Join(", ", serverAddressesFeature.Addresses)}\r\n");
                }

                await context.Response.WriteAsync($"Local connection info: {context.Connection.LocalIpAddress}:{context.Connection.LocalPort}\r\n");
                await context.Response.WriteAsync($"Remote connection info: {context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}\r\n\r\n");

                var requestUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
                await context.Response.WriteAsync($"Request URL: {requestUrl}");
            });
        }
    }
}
