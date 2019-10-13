using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PkceClient.AspNetCore3
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

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
                    options.ResponseType = "code";
                    options.ResponseMode = "form_post";
                    options.CallbackPath = "/signin-oidc";

                    // Enable PKCE (authorization code flow only)
                    options.UsePkce = true;
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}
