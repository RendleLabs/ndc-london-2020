using ChattyServerAuth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;

namespace ChattyServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DevelopmentModeCertificateHelper.Initialize("server.pfx");
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(kestrel =>
                    {
                        kestrel.ConfigureHttpsDefaults(options =>
                        {
                            options.ServerCertificate = DevelopmentModeCertificateHelper.Certificate;
                            options.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                            options.ClientCertificateValidation = (certificate2, chain, arg3) => true;
                        });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}