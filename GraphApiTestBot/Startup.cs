// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using GraphApiTestBot.Bots;
using Microsoft.AspNetCore.Mvc;
using GraphApiTestBot.Storage;
using GraphApiTestBot.Middleware;

namespace GraphApiTestBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        private IConfiguration Configuration { get; }

        private IHostingEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);

            //// Only need a single storage instance unless you really are storing your conversation state and user state in two completely DB instances
            //var storage = new CosmosDbStorage(new CosmosDbStorageOptions
            //{
            //    // … set options here …
            //});

            // do not use in memory storage for production!
            var authTokenStorage = new InMemoryAuthTokenStorage();
            services.AddSingleton<IAuthTokenStorage>(authTokenStorage);

            services.AddBot<Bot>((options) => {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

                // add azure ad auth middleware to get an authorization token to use to connect to Office 365 Graph
                options.Middleware.Add(new AzureAdAuthMiddleware(authTokenStorage, Configuration));
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseBotFramework();
        }
    }
}
