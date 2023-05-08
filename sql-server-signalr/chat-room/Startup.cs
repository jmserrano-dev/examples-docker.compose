// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR()
                .AddSqlServer(o =>
                {
                    o.ConnectionString = Configuration.GetConnectionString("Default");
                    // See above - attempts to enable Service Broker on the database at startup
                    // if not already enabled. Default false, as this can hang if the database has other sessions.
                    o.AutoEnableServiceBroker = true;
                    // Every hub has its own message table(s). 
                    // This determines the part of the table named that is derived from the hub name.
                    // IF THIS IS NOT UNIQUE AMONG ALL HUBS, YOUR HUBS WILL COLLIDE AND MESSAGES MIX.
                    o.TableSlugGenerator = hubType => hubType.Name;
                    // The number of tables per Hub to use. Adding a few extra could increase throughput
                    // by reducing table contention, but all servers must agree on the number of tables used.
                    // If you find that you need to increase this, it is probably a hint that you need to switch to Redis.
                    o.TableCount = 1;
                    // The SQL Server schema to use for the backing tables for this backplane.
                    o.SchemaName = "signalr";
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatSampleHub>("/chat");
            });
        }
    }
}
