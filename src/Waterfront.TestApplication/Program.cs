using Waterfront.AspNetCore.Extensions;
using Waterfront.Common.Tokens.Configuration;
using Waterfront.Core.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddWaterfront()
       .ConfigureTokens(
           options => {
               options.SetIssuer("http://localhost:3001");
               options.SetLifetime(120);
           }
       )
       .ConfigureEndpoints(endpoints => endpoints.SetTokenEndpoint("/token"));

WebApplication app = builder.Build();

app.UseWaterfront();

app.MapGet("/", () => "Hello World!");

app.Run();
