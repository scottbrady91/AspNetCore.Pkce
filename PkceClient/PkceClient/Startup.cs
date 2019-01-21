using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
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

                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        // only modify requests to the authorization endpoint
                        if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            // generate code_verifier
                            var codeVerifier = CryptoRandom.CreateUniqueId(32);

                            // store codeVerifier for later use
                            context.Properties.Items.Add("code_verifier", codeVerifier);

                            // create code_challenge
                            string codeChallenge;
                            using (var sha256 = SHA256.Create())
                            {
                                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                                codeChallenge = Base64Url.Encode(challengeBytes);
                            }

                            // add code_challenge and code_challenge_method to request
                            context.ProtocolMessage.Parameters.Add("code_challenge", codeChallenge);
                            context.ProtocolMessage.Parameters.Add("code_challenge_method", "S256");
                        }

                        return Task.CompletedTask;
                    };

                    options.Events.OnAuthorizationCodeReceived = context =>
                    {
                        // only when authorization code is being swapped for tokens
                        if (context.TokenEndpointRequest?.GrantType == OpenIdConnectGrantTypes.AuthorizationCode)
                        {
                            // get stored code_verifier
                            if (context.Properties.Items.TryGetValue("code_verifier", out var codeVerifier))
                            {
                                // add code_verifier to token request
                                context.TokenEndpointRequest.Parameters.Add("code_verifier", codeVerifier);
                            }
                        }

                        return Task.CompletedTask;
                    };
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
