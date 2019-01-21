using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace PkceClient
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "cookie";
                    options.DefaultSignInScheme = "cookie";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("cookie")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false; // dev only

                    options.ClientId = "pkce_client";
                    options.ClientSecret = "acf2ec6fb01a4b698ba240c2b10a0243";
                    options.ResponseType = "code id_token";
                    options.ResponseMode = "form_post";
                    options.CallbackPath = "/signin-oidc";
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
