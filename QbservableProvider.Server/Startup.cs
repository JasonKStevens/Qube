using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace QbservableProvider.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<QueryService>();
            });

            lifetime.ApplicationStopping.Register(OnShutdown);
        }

        private void OnShutdown()
        {
            // TODO: Cancel any subscriptions gracefully
        }
    }
}
