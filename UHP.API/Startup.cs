using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UHP.API
{
    public class Startup
    {
        private UdpClient udpClient;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            enableUpdListener();
        }

        private void enableUpdListener()
        {
            udpClient = Lib.SocketHelper.BuildUdpClientBind(444);

            udpClient.BeginReceive(receivedCallBack, null);
        }

        private void receivedCallBack(IAsyncResult ar)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 444);
            var data = udpClient.EndReceive(ar, ref endPoint);

            udpClient.Send(data.Reverse().ToArray(), data.Length, endPoint);

            // start again
            udpClient.BeginReceive(receivedCallBack, null);
        }
    }
}
