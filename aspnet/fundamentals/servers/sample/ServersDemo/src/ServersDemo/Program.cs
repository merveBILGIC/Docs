using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Server;

namespace ServersDemo
{
    /// <summary>
    /// Executing the "dotnet run --server [Kestrel|WebListener]" command in the application folder will run this app.
    /// You can also run "dotnet publish" and specify the publish directory as the physical path of a site in IIS Manager.
    /// </summary>
    public class Program
    {
        public static string Server;

        public static int Main(string[] args)
        {
            // Add command line configuration source to read command line parameters.
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            Server = config["server"] ?? "Kestrel";

            var builder = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();

            // The default url is http://localhost:5000 if none is specified.
            // Replace "localhost" with "*" to listen to external requests.
            if (config["address"] != null)
            {
                builder.UseUrls(config["address"]);
            }

            // If this app isn't hosted by IIS, UseIISIntegration() no-ops.
            // It isn't possible to both listen to requests directly and from IIS using the same WebHost,
            // since this will clobber your UseUrls() configuration when hosted by IIS.
            // If UseIISIntegration() is called before UseUrls(), IIS hosting will fail.
            builder.UseIISIntegration();

            if (string.Equals(Server, "Kestrel", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Running demo with Kestrel.");

                builder.UseKestrel(options =>
                {
                    if (config["threadCount"] != null)
                    {
                        options.ThreadCount = int.Parse(config["threadCount"]);
                    }
                });
            }
            else if (string.Equals(Server, "WebListener", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Running demo with WebListener.");

                builder.UseWebListener(options =>
                {
                    // AllowAnonymous is the default WebListner configuration
                    options.Listener.AuthenticationManager.AuthenticationSchemes =
                        AuthenticationSchemes.AllowAnonymous;
                });
            }
            else
            {
                Console.WriteLine($"Error: Unknown server value: '{Server}'. The valid server options are 'Kestrel' and 'WebListener'.");
                Console.WriteLine("IIS cannot be specified at runtime since it does not support self-hosting.");
                return 1;
            }

            // ASPNETCORE_PORT is the port that IIS proxies requests to.
            if (Environment.GetEnvironmentVariable($"ASPNETCORE_PORT") != null)
            {
                if (string.Equals(Server, "WebListener", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Error: WebListener cannot be used with IIS");
                    return 1;
                }

                Server = "IIS/Kestrel";

                Console.WriteLine("Hosted by IIS.");
            }

            var host = builder.Build();
            host.Run();

            return 0;
        }
    }
}
