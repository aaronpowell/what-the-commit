using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Security;
using Microsoft.AspNet.Security.Cookies;
using Microsoft.AspNet.Security.OAuth;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WhatTheCommit
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Setup configuration sources.
            Configuration = new Configuration()
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IConfiguration>(_ => Configuration);

            services.AddMvc();

            services.Configure<ExternalAuthenticationOptions>(options =>
            {
                options.SignInAsAuthenticationType = CookieAuthenticationDefaults.AuthenticationType;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940

            // Configure the HTTP request pipeline.
            // Add the console logger.
            loggerfactory.AddConsole();

            // Add the following to the request pipeline only in development environment.
            if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
            {
                app.UseBrowserLink();
                app.UseErrorPage(ErrorPageOptions.ShowAll);
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // send the request to the following path or controller action.
                app.UseErrorHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            app.UseCookieAuthentication(options =>
            {
                options.LoginPath = new PathString("/login");
                options.SlidingExpiration = true;
            });

            //security
            app.UseOAuthAuthentication("Github", options =>
            {
                options.ClientId = Configuration.Get("GithubClientId");
                options.ClientSecret = Configuration.Get("GithubClientSecret");
                options.CallbackPath = new PathString("/github-logged-in");
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";
                // Retrieving user information is unique to each provider.
                options.Notifications = new OAuthAuthenticationNotifications()
                {
                    OnGetUserInformationAsync = async (context) =>
                    {
                        // Get the GitHub user
                        var userRequest = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                        userRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        var userResponse = await context.Backchannel.SendAsync(userRequest, context.HttpContext.RequestAborted);
                        userResponse.EnsureSuccessStatusCode();
                        var text = await userResponse.Content.ReadAsStringAsync();
                        var user = JObject.Parse(text);

                        var identity = new ClaimsIdentity(
                            context.Options.AuthenticationType,
                            ClaimsIdentity.DefaultNameClaimType,
                            ClaimsIdentity.DefaultRoleClaimType);

                        identity.AddClaim(new Claim(ClaimTypes.Authentication, context.AccessToken, ClaimValueTypes.String, context.Options.AuthenticationType));

                        JToken value;
                        var id = user.TryGetValue("id", out value) ? value.ToString() : null;
                        if (!string.IsNullOrEmpty(id))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id, ClaimValueTypes.String, context.Options.AuthenticationType));
                        }
                        var userName = user.TryGetValue("login", out value) ? value.ToString() : null;
                        if (!string.IsNullOrEmpty(userName))
                        {
                            identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, userName, ClaimValueTypes.String, context.Options.AuthenticationType));
                        }
                        var name = user.TryGetValue("name", out value) ? value.ToString() : null;
                        if (!string.IsNullOrEmpty(name))
                        {
                            identity.AddClaim(new Claim("urn:github:name", name, ClaimValueTypes.String, context.Options.AuthenticationType));
                        }
                        var link = user.TryGetValue("url", out value) ? value.ToString() : null;
                        if (!string.IsNullOrEmpty(link))
                        {
                            identity.AddClaim(new Claim("urn:github:url", link, ClaimValueTypes.String, context.Options.AuthenticationType));
                        }

                        context.Identity = identity;
                    },
                };
            });

            app.Map("/github-login", signoutApp =>
            {
                signoutApp.Run(async context =>
                {
                    context.Response.Challenge(new AuthenticationProperties() {
                        RedirectUri = "/"
                    }, "Github");
                    await Task.Yield();
                });
            });

            app.Map("/logout", signoutApp =>
            {
                signoutApp.Run(async context =>
                {
                    context.Response.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                    context.Response.Redirect("/");
                    await Task.Yield();
                });
            });

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                // Uncomment the following line to add a route for porting Web API 2 controllers.
            });
        }
    }
}
